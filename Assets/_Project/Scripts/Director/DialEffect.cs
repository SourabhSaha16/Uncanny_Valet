namespace UncannyValet.Director
{
    /// <summary>How a <see cref="DialLink"/> combines with the key's starting value.</summary>
    public enum DialEffect
    {
        /// <summary>Multiplies the distance from the pivot: value = pivot + (start - pivot) x anchor.</summary>
        Scale,
        /// <summary>Adds the anchor to the start value.</summary>
        Add,
    }
}
