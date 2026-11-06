namespace UncannyValet.Patrons
{
    /// <summary>EV charging: fills only on minutes the car sits in a charging bay.</summary>
    public sealed class ChargeRule
    {
        private readonly ServiceProgress _progress;

        public ChargeRule(float startCharge, float chargeMinutes) { _progress = new ServiceProgress(startCharge, chargeMinutes); }

        public float Charge01 => _progress.Value;
        public bool IsFull => _progress.IsFull;

        /// <summary>One game minute. Returns true if the charge changed.</summary>
        public bool Advance(bool inChargingBay) => inChargingBay && _progress.Step();
    }
}
