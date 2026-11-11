namespace UncannyValet.Patrons
{
    /// <summary>Washing a dirty car: starts at 0, fills only in the wash bay, keeps progress when the car leaves.</summary>
    public sealed class WashRule
    {
        private readonly ServiceProgress _progress;

        public WashRule(float washMinutes) { _progress = new ServiceProgress(0f, washMinutes); }

        public float Clean01 => _progress.Value;
        public bool IsClean => _progress.IsFull;

        /// <summary>Washes the car spotless at once (Park Smash). Returns true if the clean level changed.</summary>
        public bool Complete() => _progress.Complete();

        /// <summary>One game minute. Returns true if the clean level changed.</summary>
        public bool Advance(bool inWashBay) => inWashBay && _progress.Step();
    }
}
