namespace UncannyValet.Core.Events
{
    /// <summary>The rules for the shift: which challenges are on and how busy it is.</summary>
    public readonly struct ShiftPlan
    {
        /// <summary>Most cars that may wait for parking at once.</summary>
        public readonly int MaxWaitingCars;
        /// <summary>Multiplies the gap between arrivals (below 1 = busier).</summary>
        public readonly float ArrivalGapScale;
        public readonly bool ChargingOpen;
        public readonly bool WashOpen;
        /// <summary>How many parking spots are open, counted from the curb. 0 means all of them.</summary>
        public readonly int ActiveSpots;
        /// <summary>Chance reached at the end of the shift by the modifiers that ramp late (NeedsCharge); it ramps from the lot's chance once the shift passes LateEvStart.</summary>
        public readonly float LateEvChance;
        /// <summary>Fraction 0..1 of the shift after which the late-ramping chance moves toward LateEvChance.</summary>
        public readonly float LateEvStart;

        public ShiftPlan(int maxWaitingCars, float arrivalGapScale,
            bool chargingOpen, bool washOpen, int activeSpots = 0, float lateEvChance = 0.5f, float lateEvStart = 0.6f)
        {
            MaxWaitingCars = maxWaitingCars; ArrivalGapScale = arrivalGapScale;
            ChargingOpen = chargingOpen;
            WashOpen = washOpen; ActiveSpots = activeSpots;
            LateEvChance = lateEvChance; LateEvStart = lateEvStart;
        }
    }
}
