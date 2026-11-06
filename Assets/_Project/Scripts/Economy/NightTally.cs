using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Economy
{
    /// <summary>
    /// Pure per-night counters. Every ticket lands in exactly one bucket (parked or not parked), and
    /// nothing is counted after <see cref="End"/>, so the order of NightEnded subscribers cannot matter.
    /// </summary>
    public sealed class NightTally
    {
        private readonly HashSet<CarId> _parked = new HashSet<CarId>();
        private readonly Dictionary<TicketId, CarId> _open = new Dictionary<TicketId, CarId>();
        private readonly int _totalNights;
        private int _night;
        private int _debtAtStart;

        public bool IsActive { get; private set; }
        public int Tips { get; private set; }
        public int CarsParked => _parked.Count;
        public int CarsNotParked { get; private set; }
        public int BaseTips { get; private set; }
        public int SpeedAdjust { get; private set; }
        public int ChargeBonus { get; private set; }
        public int CleanBonus { get; private set; }
        public int ZeroedTips { get; private set; }

        public NightTally(int totalNights) => _totalNights = Math.Max(1, totalNights);

        /// <summary>Resets every counter and remembers the debt this night starts with.</summary>
        public void Start(int night, int debtAtStart)
        {
            _parked.Clear();
            _open.Clear();
            _night = night;
            _debtAtStart = Math.Max(0, debtAtStart);
            Tips = 0;
            CarsNotParked = 0;
            BaseTips = SpeedAdjust = ChargeBonus = CleanBonus = ZeroedTips = 0;
            IsActive = true;
        }

        /// <summary>Adds one ticket's tip parts to the breakdown totals.</summary>
        public void RecordTipExplained(in TipBreakdown breakdown)
        {
            if (!IsActive) return;
            BaseTips += breakdown.BaseTip;
            SpeedAdjust += breakdown.SpeedAdjust;
            ChargeBonus += breakdown.ChargeBonus;
            CleanBonus += breakdown.CleanBonus;
            if (breakdown.ZeroReason != TipZeroReason.None) ZeroedTips++;
        }

        public void RecordTicketOpened(TicketId ticket, CarId car)
        {
            if (IsActive) _open[ticket] = car;
        }

        public void RecordCarParked(CarId car)
        {
            if (IsActive) _parked.Add(car);
        }

        public void RecordTicketClosed(TicketId ticket, CarId car, int tip)
        {
            if (!IsActive) return;
            Tips += Math.Max(0, tip);
            _open.Remove(ticket);
            if (!_parked.Contains(car)) CarsNotParked++;
        }

        /// <summary>Counts still-open never-parked tickets as not parked, stops counting and returns the summary
        /// carrying the given note about tomorrow.</summary>
        public NightSummary End(string nextNightNote = "")
        {
            foreach (var pair in _open)
                if (!_parked.Contains(pair.Value)) CarsNotParked++;
            _open.Clear();
            IsActive = false;
            return new NightSummary(_night, _totalNights, Tips, CarsParked, CarsNotParked,
                Math.Max(0, _debtAtStart - Tips), BaseTips, SpeedAdjust, ChargeBonus, CleanBonus, ZeroedTips,
                nextNightNote);
        }
    }
}
