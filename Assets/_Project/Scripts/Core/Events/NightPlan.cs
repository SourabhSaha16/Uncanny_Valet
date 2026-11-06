namespace UncannyValet.Core.Events
{
    /// <summary>The rules for one night: which challenges are on and how busy it is.</summary>
    public readonly struct NightPlan
    {
        public readonly int Night;
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
        /// <summary>What is new tonight; shown on the previous night's end card.</summary>
        public readonly string NewTonight;
        /// <summary>How many parking spots are open tonight, counted from the curb. 0 means all of them.</summary>
        public readonly int ActiveSpots;
        /// <summary>EV chance reached at the end of the night; the chance ramps from EvChance once the night passes LateEvStart.</summary>
        public readonly float LateEvChance;
        /// <summary>Fraction 0..1 of the night after which the EV chance ramps toward LateEvChance.</summary>
        public readonly float LateEvStart;

        public NightPlan(int night, int maxWaitingCars, float arrivalGapScale, float evChance, float dirtyChance,
            bool chargingOpen, bool washOpen, string newTonight, int activeSpots = 0,
            float lateEvChance = -1f, float lateEvStart = 0.6f)
        {
            Night = night; MaxWaitingCars = maxWaitingCars; ArrivalGapScale = arrivalGapScale;
            EvChance = evChance; DirtyChance = dirtyChance; ChargingOpen = chargingOpen;
            WashOpen = washOpen; NewTonight = newTonight ?? ""; ActiveSpots = activeSpots;
            LateEvChance = lateEvChance < 0f ? evChance : lateEvChance; LateEvStart = lateEvStart;
        }
    }
}
