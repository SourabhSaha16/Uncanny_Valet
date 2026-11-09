using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Reads a live tuning number, keeping the current value when the key was never seeded.</summary>
    public static class TuningRead
    {
        public static float Float(ITuning tuning, TuningKey key, float current)
            => tuning.TryGet(key, out var value) ? value : current;
    }
}
