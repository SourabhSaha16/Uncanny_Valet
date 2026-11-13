namespace UncannyValet.Patrons
{
    /// <summary>Numbers for the lot services of tickets created from now on; built from PatronConfig and the live tuning. The per-modifier numbers live in the modifier definitions.</summary>
    public readonly struct ServiceTuning
    {
        /// <summary>Game minutes a car may sit in the arrival queue before its stay starts anyway (PatronConfig default).</summary>
        public const float DefaultQueueWaitMinutes = 3f;

        public readonly float QueueWaitMinutes;

        public ServiceTuning(float queueWaitMinutes = DefaultQueueWaitMinutes)
        {
            QueueWaitMinutes = queueWaitMinutes;
        }

        public static ServiceTuning Default => new ServiceTuning(DefaultQueueWaitMinutes);
    }
}
