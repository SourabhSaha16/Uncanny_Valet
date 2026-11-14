using NUnit.Framework;
using UncannyValet.Cars;

namespace UncannyValet.Tests.Cars
{
    /// <summary>The car's speed after it hits a pedestrian: 15% slower, never below 30% of top speed.</summary>
    public class PedestrianSlowRuleTests
    {
        [Test]
        public void LosesTheShareOfItsSpeed()
            => Assert.AreEqual(6.8f, PedestrianSlowRule.SlowedSpeed(8f, 0.15f, 2.4f), 0.0001f);

        [Test]
        public void NeverDropsBelowTheFloor()
            => Assert.AreEqual(2.4f, PedestrianSlowRule.SlowedSpeed(2.5f, 0.15f, 2.4f), 0.0001f);

        [Test]
        public void ACarAlreadyBelowTheFloorKeepsItsSpeed()
            => Assert.AreEqual(1f, PedestrianSlowRule.SlowedSpeed(1f, 0.15f, 2.4f), 0.0001f);

        [Test]
        public void ZeroShareChangesNothing()
            => Assert.AreEqual(5f, PedestrianSlowRule.SlowedSpeed(5f, 0f, 2.4f), 0.0001f);
    }
}
