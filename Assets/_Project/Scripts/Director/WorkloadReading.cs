namespace UncannyValet.Director
{
    /// <summary>The player's owed work and urgency at one moment.</summary>
    public readonly struct WorkloadReading
    {
        /// <summary>Seconds of work the player owes.</summary>
        public readonly float Seconds;

        /// <summary>Workload W: owed seconds over the horizon. 1 = exactly busy, above 1.3 drowning, below 0.3 idle.</summary>
        public readonly float W;

        /// <summary>Pressure P: 1 minus the smallest patience fraction among waiting and pickup tickets (0 when none).</summary>
        public readonly float P;

        public WorkloadReading(float seconds, float w, float p) { Seconds = seconds; W = w; P = p; }
    }
}
