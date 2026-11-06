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

        public bool IsFinalNight => Night >= TotalNights;
        public bool PaidOff => DebtLeft <= 0;

        public NightSummary(int night, int totalNights, int tips, int carsParked, int carsNotParked, int debtLeft)
        {
            Night = night; TotalNights = totalNights; Tips = tips;
            CarsParked = carsParked; CarsNotParked = carsNotParked; DebtLeft = debtLeft;
        }
    }
}
