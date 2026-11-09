namespace UncannyValet.Director
{
    /// <summary>The director's random source. Bootstrap supplies the per-shift seeded generator.</summary>
    public interface IDirectorRandom
    {
        /// <summary>A value in [0, 1).</summary>
        float Value();

        float Range(float min, float max);
    }
}
