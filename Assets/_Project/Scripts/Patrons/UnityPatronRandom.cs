namespace UncannyValet.Patrons
{
    public sealed class UnityPatronRandom : IPatronRandom
    {
        public float Value() => UnityEngine.Random.value;
        public float Range(float min, float max) => UnityEngine.Random.Range(min, max);
    }
}
