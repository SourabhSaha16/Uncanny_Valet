namespace UncannyValet.Core.Events
{
    /// <summary>The rules for the shift: which challenges are on and how busy it is.</summary>
    public readonly struct ShiftPlan
    {
        /// <summary>Most cars that may wait for parking at once.</summary>
        public readonly int MaxWaitingCars;
        /// <summary>Multiplies the gap between arrivals (below 1 = busier).</summary>
        public readonly float ArrivalGapScale;
        /// <summary>Chance 0..1 that a new car is an EV.</summary>
        public readonly float EvChance;
        /// <summary>Chance 0..1 that a new car is dirty.</summary>
        public readonly float DirtyChance;
        public readonly bool ChargingOpen;
        public readonly bool WashOpen;
        /// <summary>How many parking spots are open, counted from the curb. 0 means all of them.</summary>
        public readonly int ActiveSpots;
        /// <summary>EV chance reached at the end of the shift; the chance ramps from EvChance once the shift passes LateEvStart.</summary>
        public readonly float LateEvChance;
        /// <summary>Fraction 0..1 of the shift after which the EV chance ramps toward LateEvChance.</summary>
        public readonly float LateEvStart;

        public ShiftPlan(int maxWaitingCars, float arrivalGapScale, float evChance, float dirtyChance,
            bool chargingOpen, bool washOpen, int activeSpots = 0, float lateEvChance = -1f, float lateEvStart = 0.6f)
        {
            MaxWaitingCars = maxWaitingCars; ArrivalGapScale = arrivalGapScale;
            EvChance = evChance; DirtyChance = dirtyChance; ChargingOpen = chargingOpen;
            WashOpen = washOpen; ActiveSpots = activeSpots;
            LateEvChance = lateEvChance < 0f ? evChance : lateEvChance; LateEvStart = lateEvStart;
        }
    }
}
