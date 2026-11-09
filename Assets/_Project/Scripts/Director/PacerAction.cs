namespace UncannyValet.Director
{
    /// <summary>What the arrival pacer wants this tick.</summary>
    public enum PacerAction
    {
        /// <summary>Keep counting down.</summary>
        Wait,

        /// <summary>Do not send a car now (see <see cref="HoldReason"/>).</summary>
        Hold,

        /// <summary>Send a car now.</summary>
        Spawn
    }
}
