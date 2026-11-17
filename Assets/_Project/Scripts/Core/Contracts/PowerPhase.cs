namespace UncannyValet.Core.Contracts
{
    /// <summary>Where an owned power is in its use cycle.</summary>
    public enum PowerPhase
    {
        /// <summary>Not running and not cooling down: it can be fired when a bucket is open.</summary>
        Ready,
        /// <summary>Running now (a power with a duration).</summary>
        Active,
        /// <summary>Its use ended; waiting for the cooldown.</summary>
        Cooling,
    }
}
