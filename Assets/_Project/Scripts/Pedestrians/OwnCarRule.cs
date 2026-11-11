using UncannyValet.Core.Ids;

namespace UncannyValet.Pedestrians
{
    /// <summary>Pure rule: a patron on foot never collides with, bumps or waits for the car tied to their ticket.</summary>
    public static class OwnCarRule
    {
        /// <summary>True when <paramref name="other"/> is the car <paramref name="mine"/> (an ambient person has no car, so never).</summary>
        public static bool IsOwn(CarId mine, CarId other) => mine.IsValid && mine == other;
    }
}
