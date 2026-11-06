namespace UncannyValet.Core.Events
{
    /// <summary>A tuning value was changed at runtime. Owners re-read it from ITuning and apply it.</summary>
    public readonly struct TuningChanged : IGameEvent
    {
        public readonly TuningKey Key;
        public readonly float Value;
        public TuningChanged(TuningKey key, float value) { Key = key; Value = value; }
    }
}
