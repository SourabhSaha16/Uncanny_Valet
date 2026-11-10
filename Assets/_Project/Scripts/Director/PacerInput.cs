namespace UncannyValet.Director
{
    /// <summary>Everything the arrival pacer reads in one tick.</summary>
    public readonly struct PacerInput
    {
        /// <summary>Workload W.</summary>
        public readonly float Workload;
        /// <summary>Pressure P.</summary>
        public readonly float Pressure;
        /// <summary>Target intensity T at this moment.</summary>
        public readonly float Target;
        /// <summary>Form F, -1..1.</summary>
        public readonly float Form;
        /// <summary>Angry patrons within the relief window.</summary>
        public readonly int AngryRecent;
        /// <summary>Cars waiting to be parked (arriving, waiting or being driven there).</summary>
        public readonly int Queue;
        /// <summary>The most cars that may wait at once.</summary>
        public readonly int QueueCap;
        /// <summary>Cars delivered so far this shift.</summary>
        public readonly int ArrivalsUsed;
        /// <summary>Area under T from now to the end of the shift, in workload-seconds.</summary>
        public readonly float TargetAreaLeft;
        /// <summary>Area under T over the whole shift, in workload-seconds; with the area left it tells how far the shift has got.</summary>
        public readonly float TargetAreaTotal;
        /// <summary>Shortest base interval, seconds (the old 15-25 s range, scaled by the plan).</summary>
        public readonly float BaseIntervalMin;
        /// <summary>Longest base interval, seconds.</summary>
        public readonly float BaseIntervalMax;

        /// <summary>The pacing gap: workload minus the target nudged by form (spec section 3).</summary>
        public float Gap(float formInfluence) => Workload - (Target + formInfluence * Form);

        public PacerInput(float workload, float pressure, float target, float form, int angryRecent, int queue,
            int queueCap, int arrivalsUsed, float targetAreaLeft, float targetAreaTotal, float baseIntervalMin,
            float baseIntervalMax)
        {
            Workload = workload; Pressure = pressure; Target = target; Form = form; AngryRecent = angryRecent;
            Queue = queue; QueueCap = queueCap; ArrivalsUsed = arrivalsUsed; TargetAreaLeft = targetAreaLeft;
            TargetAreaTotal = targetAreaTotal;
            BaseIntervalMin = baseIntervalMin; BaseIntervalMax = baseIntervalMax;
        }
    }
}
