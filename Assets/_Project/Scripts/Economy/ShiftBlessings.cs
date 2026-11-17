using System;
using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Economy
{
    /// <summary>
    /// Owns the shift's blessings, favour and curses (the run state owner that replaces the debt ledger). A blessing
    /// adds its amount and raises the favour; a blessing halved for a missed service leaves the favour where it is; a zero blessing resets it; a curse (an angry patron: the pickup
    /// timer ran out) takes blessings off, never below 0, and resets the favour. Everything starts clean on
    /// ShiftStarted. At ShiftEnded the lot is judged against its threshold: ShiftResultReady goes out, then
    /// LotCleared or LotFailed.
    /// </summary>
    public sealed class ShiftBlessings : IShiftBlessingsView, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ShiftConfig _config;
        private readonly FavourTracker _favour;
        private readonly Func<ILotGoal> _goal;
        private readonly PedestrianCurseRule _pedestrianCurses = new PedestrianCurseRule();
        private readonly SlightCurseRule _slightCurses = new SlightCurseRule();
        private readonly OvertimeCurseRule _overtime = new OvertimeCurseRule();
        private bool _open;

        /// <summary>Overtime curse numbers; null uses the rule's defaults.</summary>
        public OvertimeCurseConfig OvertimeConfig { get; set; }

        /// <summary>Seconds clock the pedestrian curse lockout reads (the game time by default; tests replace it).</summary>
        public Func<float> Clock { get; set; } = () => Time.time;

        public int Blessings { get; private set; }
        public int Curses { get; private set; }
        public int PeakFavour { get; private set; } = 1;
        public int FavourMultiplier => _favour.Multiplier;

        /// <param name="goal">Supplies the lot goal when the shift ends; null (or a null result) judges nothing and publishes no lot event.</param>
        public ShiftBlessings(IEventBus bus, ShiftConfig config, Func<ILotGoal> goal = null)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _goal = goal;
            _favour = new FavourTracker(config.MaxFavourMultiplier);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
            _bus.Subscribe<BlessingEarned>(OnBlessingEarned);
            _bus.Subscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Subscribe<PedestrianHit>(OnPedestrianHit);
            _bus.Subscribe<CarDelivered>(OnCarDelivered);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<BlessingEarned>(OnBlessingEarned);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Unsubscribe<PedestrianHit>(OnPedestrianHit);
            _bus.Unsubscribe<CarDelivered>(OnCarDelivered);
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
        }

        /// <summary>
        /// A curse from any source: takes <paramref name="amount"/> blessings off (never below 0), resets the favour and
        /// publishes <see cref="CurseApplied"/>. A source passes its own amount or <see cref="ShiftConfig.DefaultCurse"/>.
        /// </summary>
        public void ApplyCurse(TicketId ticket, int amount)
        {
            if (!_open) return;
            Curses++;
            Blessings = Math.Max(0, Blessings - Math.Max(0, amount));
            _favour.Reset();
            _bus.Publish(new CurseApplied(ticket, amount));
            Publish();
        }

        private void OnShiftStarted(ShiftStarted e)
        {
            Blessings = 0;
            Curses = 0;
            _favour.Reset();
            PeakFavour = 1;
            _open = true;
            _pedestrianCurses.Reset();
            _slightCurses.Reset();
            _overtime.Reset();
            Publish();
        }

        private void OnShiftEnded(ShiftEnded e)
        {
            if (!_open) return;
            _open = false;
            var goal = _goal?.Invoke();
            var threshold = goal?.BlessingThreshold ?? 0;
            var cleared = goal != null && Blessings >= threshold;
            _bus.Publish(new ShiftResultReady(new ShiftResult(Blessings, threshold, cleared, Curses, PeakFavour)));
            if (goal == null) return;
            if (cleared) _bus.Publish(new LotCleared(goal.Lot));
            else _bus.Publish(new LotFailed(goal.Lot));
        }

        private void OnBlessingEarned(BlessingEarned e)
        {
            if (!_open) return;
            if (e.Amount > 0)
            {
                Blessings += e.Amount;
                if (!e.ServiceMissed) RaiseFavour();
            }
            else
            {
                _favour.Reset();
            }
            Publish();
        }

        private void RaiseFavour()
        {
            _favour.RecordHappy();
            PeakFavour = Math.Max(PeakFavour, _favour.Multiplier);
        }

        // The pickup timer running out means an angry patron: the blessing is 0 and the default curse lands.
        private void OnTimerExpired(TicketTimerExpired e)
        {
            _overtime.MarkExpired(e.Ticket, Clock());
            ApplyCurse(e.Ticket, _config.DefaultCurse);
        }

        // The late car finally reaches its angry patron: a further, milder curse that grows with the wait beyond the timer.
        private void OnCarDelivered(CarDelivered e)
        {
            var seconds = OvertimeConfig != null ? OvertimeConfig.SecondsPerPoint : OvertimeCurseRule.DefaultSecondsPerPoint;
            var cap = OvertimeConfig != null ? OvertimeConfig.Cap : OvertimeCurseRule.DefaultCap;
            var amount = _overtime.Take(e.Ticket, Clock(), seconds, cap);
            if (amount > 0) ApplyCurse(e.Ticket, amount);
        }

        private void OnTicketClosed(TicketClosed e) => _overtime.Forget(e.Ticket);

        // The driven car hit a walker (no ticket belongs to the curse). A patron where the lot allows it gets the full curse;
        // anyone else, on every lot, the slight one. One curse per hit: the bigger wins, so a patron the full curse covers
        // never also gets the slight one (not even during the full lockout).
        private void OnPedestrianHit(PedestrianHit e)
        {
            if (!_open || !Services.TryGet<ITuning>(out var tuning)) return;
            var on = tuning.TryGet(TuningKey.PedestrianCurses, out var switchValue) && switchValue >= 0.5f;
            var lockout = tuning.TryGet(TuningKey.PedestrianCurseLockout, out var l) ? l : PedestrianHitRows.CurseLockout;
            if (_pedestrianCurses.TryCurse(e, Clock(), on, lockout))
            {
                var amount = tuning.TryGet(TuningKey.PedestrianCurseAmount, out var a) ? a : PedestrianHitRows.CurseAmount;
                ApplyCurse(TicketId.None, Mathf.RoundToInt(amount));
                return;
            }
            if (PedestrianCurseRule.Covers(e, on) || !_slightCurses.TryCurse(e, Clock(), lockout)) return;
            var slight = tuning.TryGet(TuningKey.PedestrianSlightCurseAmount, out var s) ? s : PedestrianHitRows.SlightCurseAmount;
            ApplyCurse(TicketId.None, Mathf.RoundToInt(slight));
        }

        private void Publish() => _bus.Publish(new BlessingsChanged(Blessings, FavourMultiplier, Curses));
    }
}
