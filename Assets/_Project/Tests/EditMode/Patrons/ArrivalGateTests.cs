using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class ArrivalGateTests
    {
        [Test]
        public void BlockedByWaitingLimit() => Assert.IsFalse(ArrivalGate.CanArrive(2, 2, false));

        [Test]
        public void BlockedByArrivalPoint() => Assert.IsFalse(ArrivalGate.CanArrive(0, 2, true));

        [Test]
        public void AllowedWhenFewWaitingAndPointClear() => Assert.IsTrue(ArrivalGate.CanArrive(1, 2, false));
    }
}
