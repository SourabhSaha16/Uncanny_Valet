namespace UncannyValet.Core.Events
{
    /// <summary>What happened in one night, shown on the end-of-night card.</summary>
    public readonly struct NightSummary
    {
        public readonly int Night;
        public readonly int TotalNights;
        public readonly int Tips;
        /// <summary>Cars that were parked at least once.</summary>
        public readonly int CarsParked;
        /// <summary>Cars that were never parked: they vanished, or were still waiting when the night ended.</summary>
        public readonly int CarsNotParked;
        /// <summary>Debt left after tonight's tips were deducted.</summary>
        public readonly int DebtLeft;

        // The tip calculation, summed over tonight's tickets.
        public readonly int BaseTips;
        /// <summary>Speed bonuses minus wait penalties.</summary>
        public readonly int SpeedAdjust;
        public readonly int ChargeBonus;
        public readonly int CleanBonus;
        /// <summary>Tickets whose tip was forced to $0 (late pickup or unwashed).</summary>
        public readonly int ZeroedTips;

        /// <summary>What is new tomorrow (from the next night's plan); empty on the final night.</summary>
        public readonly string NextNightNote;

        /// <summary>The run ended tonight because the strike limit was reached (boss orders).</summary>
        public readonly bool RunLost;

        public bool IsFinalNight => Night >= TotalNights || RunLost;
        public bool PaidOff => DebtLeft <= 0;

        public NightSummary(int night, int totalNights, int tips, int carsParked, int carsNotParked, int debtLeft,
            int baseTips = 0, int speedAdjust = 0, int chargeBonus = 0, int cleanBonus = 0, int zeroedTips = 0,
            string nextNightNote = "", bool runLost = false)
        {
            Night = night; TotalNights = totalNights; Tips = tips;
            CarsParked = carsParked; CarsNotParked = carsNotParked; DebtLeft = debtLeft;
            BaseTips = baseTips; SpeedAdjust = speedAdjust; ChargeBonus = chargeBonus;
            CleanBonus = cleanBonus; ZeroedTips = zeroedTips; NextNightNote = nextNightNote ?? ""; RunLost = runLost;
        }
    }
}
