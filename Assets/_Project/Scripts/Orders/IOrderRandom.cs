namespace UncannyValet.Orders
{
    /// <summary>Random source for boss orders; tests swap in a scripted one.</summary>
    public interface IOrderRandom
    {
        /// <summary>An integer in [0, maxExclusive).</summary>
        int Next(int maxExclusive);

        float Range(float min, float max);
    }
}
