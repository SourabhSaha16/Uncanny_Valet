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

        public NightPlan(int night, int maxWaitingCars, float arrivalGapScale, float evChance, float dirtyChance,
            bool chargingOpen, bool washOpen, string newTonight)
        {
            Night = night; MaxWaitingCars = maxWaitingCars; ArrivalGapScale = arrivalGapScale;
            EvChance = evChance; DirtyChance = dirtyChance; ChargingOpen = chargingOpen;
            WashOpen = washOpen; NewTonight = newTonight ?? "";
        }
    }
}
