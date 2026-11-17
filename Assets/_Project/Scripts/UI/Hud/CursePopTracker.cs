using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Starts a "-N" pop at the hit spot when a pedestrian hit costs blessings. PedestrianHit gives the spot, CurseApplied (with
    /// no ticket: a ticket curse is an angry patron, not a hit) the requested amount, and BlessingsChanged the running total, so
    /// the pop shows what was really lost. One curse per hit means one pop per hit; a hit inside the lockout has no
    /// CurseApplied and so no pop. See <see cref="CursePopRule"/>.
    /// </summary>
    public sealed class CursePopTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly BlessingPops _pops;
        private readonly BlessingPopSettings _settings;
        private readonly Func<float> _clock;
        private readonly Func<int> _frame;
        private readonly CursePopRule _rule = new CursePopRule();
        private int _blessings;

        public CursePopTracker(IEventBus bus, BlessingPops pops, BlessingPopSettings settings, Func<float> clock, Func<int> frame)
        {
            _bus = bus;
            _pops = pops;
            _settings = settings;
            _clock = clock;
            _frame = frame;
            _bus.Subscribe<BlessingsChanged>(OnBlessingsChanged);
            _bus.Subscribe<CurseApplied>(OnCurse);
            _bus.Subscribe<PedestrianHit>(OnHit);
            _bus.Subscribe<ShiftEnded>(OnShiftEnded);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<BlessingsChanged>(OnBlessingsChanged);
            _bus.Unsubscribe<CurseApplied>(OnCurse);
            _bus.Unsubscribe<PedestrianHit>(OnHit);
            _bus.Unsubscribe<ShiftEnded>(OnShiftEnded);
        }

        private void OnBlessingsChanged(BlessingsChanged e) => _blessings = e.Blessings;

        private void OnCurse(CurseApplied e)
        {
            if (e.Ticket != TicketId.None) return;
            if (_rule.Curse(CursePopRule.Lost(_blessings, e.Amount), _frame())) Start();
        }

        private void OnHit(PedestrianHit e)
        {
            if (e.ByPlayerCar && _rule.Hit(e.Point, _frame())) Start();
        }

        private void OnShiftEnded(ShiftEnded e) => _rule.Clear();

        private void Start()
        {
            _rule.Take(out var lost, out var point);
            _pops.StartCurse(new Vector2(point.x, point.y + _settings.CurseLiftWorldUnits), CursePopRule.Format(lost), _clock());
        }
    }
}
