namespace UncannyValet.Patrons
{
    /// <summary>Random source for patron rolls; tests swap in a scripted one.</summary>
    public interface IPatronRandom
    {
        /// <summary>A value in [0, 1).</summary>
        float Value();

        float Range(float min, float max);
    }
}
