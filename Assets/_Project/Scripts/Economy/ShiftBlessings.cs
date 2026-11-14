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
        private bool _open;

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
        }

        public void Dispose()
        {
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
            _bus.Unsubscribe<BlessingEarned>(OnBlessingEarned);
            _bus.Unsubscribe<TicketTimerExpired>(OnTimerExpired);
            _bus.Unsubscribe<PedestrianHit>(OnPedestrianHit);
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
        private void OnTimerExpired(TicketTimerExpired e) => ApplyCurse(e.Ticket, _config.DefaultCurse);

        // The driven car hit a patron walker: a curse of the tuned size where the lot allows it (no ticket belongs to it).
        private void OnPedestrianHit(PedestrianHit e)
        {
            if (!_open || !Services.TryGet<ITuning>(out var tuning)) return;
            var on = tuning.TryGet(TuningKey.PedestrianCurses, out var switchValue) && switchValue >= 0.5f;
            var lockout = tuning.TryGet(TuningKey.PedestrianCurseLockout, out var l) ? l : PedestrianHitRows.CurseLockout;
            if (!_pedestrianCurses.TryCurse(e, Clock(), on, lockout)) return;
            var amount = tuning.TryGet(TuningKey.PedestrianCurseAmount, out var a) ? a : PedestrianHitRows.CurseAmount;
            ApplyCurse(TicketId.None, Mathf.RoundToInt(amount));
        }

        private void Publish() => _bus.Publish(new BlessingsChanged(Blessings, FavourMultiplier, Curses));
    }
}
