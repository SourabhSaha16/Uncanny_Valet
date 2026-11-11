namespace UncannyValet.Patrons
{
    /// <summary>Numbers for the lot services; built from PatronConfig, defaults match its defaults.</summary>
    public readonly struct ServiceTuning
    {
        /// <summary>Game minutes a car may sit in the arrival queue before its stay starts anyway (PatronConfig default).</summary>
        public const float DefaultQueueWaitMinutes = 3f;

        public readonly float EvStartCharge;
        public readonly float ChargeMinutes;
        public readonly float WashMinutes;
        public readonly float QueueWaitMinutes;

        public ServiceTuning(float evStartCharge, float chargeMinutes, float washMinutes,
            float queueWaitMinutes = DefaultQueueWaitMinutes)
        {
            EvStartCharge = evStartCharge; ChargeMinutes = chargeMinutes; WashMinutes = washMinutes;
            QueueWaitMinutes = queueWaitMinutes;
        }

        public static ServiceTuning Default => new ServiceTuning(0.2f, 20f, 15f);
    }
}
