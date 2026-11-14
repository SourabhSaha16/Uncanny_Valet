namespace UncannyValet.Core.Art
{
    /// <summary>How a prop layer moves. Stored as integers in assets: append-only.</summary>
    public enum PropAnimation
    {
        None = 0,
        /// <summary>Turns around the layer's pivot (a wheel).</summary>
        Spin = 1,
        /// <summary>Switches on and off (a neon sign).</summary>
        Blink = 2,
    }
}
