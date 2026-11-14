namespace UncannyValet.Watchers
{
    /// <summary>The pure yes-or-no rules behind the tip situations and the crash comment.</summary>
    public static class SituationRules
    {
        /// <summary>True when a patience timer is at or past <paramref name="threshold01"/> of its length.</summary>
        public static bool IsPatienceNearlyGone(float elapsed01, float threshold01) => elapsed01 >= threshold01;

        /// <summary>True when every one of the lot's arrival queue slots holds a waiting car.</summary>
        public static bool IsQueueFull(int waitingCars, int queueSlots) => queueSlots > 0 && waitingCars >= queueSlots;

        /// <summary>True when one hit added more damage than <paramref name="threshold01"/> of a car.</summary>
        public static bool IsBigCrash(float hitDamage01, float threshold01) => hitDamage01 > threshold01;
    }
}
