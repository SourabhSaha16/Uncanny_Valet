namespace UncannyValet.Patrons
{
    /// <summary>Numbers for the lot services; built from PatronConfig, defaults match its defaults.</summary>
    public readonly struct ServiceTuning
    {
        public readonly float EvStartCharge;
        public readonly float ChargeMinutes;
        public readonly float WashMinutes;

        public ServiceTuning(float evStartCharge, float chargeMinutes, float washMinutes)
        {
            EvStartCharge = evStartCharge; ChargeMinutes = chargeMinutes; WashMinutes = washMinutes;
        }

        public static ServiceTuning Default => new ServiceTuning(0.2f, 20f, 15f);
    }
}
