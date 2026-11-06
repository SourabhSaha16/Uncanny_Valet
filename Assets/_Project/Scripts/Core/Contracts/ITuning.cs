using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The live value of every TuningKey. Bootstrap seeds it from the config assets; owners read it and react to
    /// TuningChanged; the debug panel writes it. Config assets are never modified.
    /// </summary>
    public interface ITuning
    {
        /// <summary>The current value. Fails loudly (Debug.LogError, returns 0) if the key was never seeded.</summary>
        float Get(TuningKey key);

        /// <summary>Same as Get but returns false instead of logging when the key was never seeded.</summary>
        bool TryGet(TuningKey key, out float value);

        /// <summary>Sets a value and publishes TuningChanged.</summary>
        void Set(TuningKey key, float value);

        /// <summary>Sets the starting value without publishing; for Bootstrap only. Also remembers it for Reset.</summary>
        void Seed(TuningKey key, float value);

        /// <summary>Puts a key back to its seeded value, publishing TuningChanged.</summary>
        void Reset(TuningKey key);
    }
}
