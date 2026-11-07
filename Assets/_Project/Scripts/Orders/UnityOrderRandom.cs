namespace UncannyValet.Orders
{
    public sealed class UnityOrderRandom : IOrderRandom
    {
        public int Next(int maxExclusive) => UnityEngine.Random.Range(0, maxExclusive);
        public float Range(float min, float max) => UnityEngine.Random.Range(min, max);
    }
}
