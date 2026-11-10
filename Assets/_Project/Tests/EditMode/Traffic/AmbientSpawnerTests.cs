using NUnit.Framework;
using UncannyValet.Traffic;

namespace UncannyValet.Tests.Traffic
{
    public class AmbientSpawnerTests
    {
        private const float Dt = 0.1f;
        private const int Looks = 5;

        private static AmbientSpawner Make(int seed) => new AmbientSpawner(seed, Looks, 0.5f, 0.5f, 0.8f, 1.2f);

        private static int Count(AmbientSpawner spawner, float rate, float seconds)
        {
            int count = 0;
            for (float t = 0f; t < seconds; t += Dt)
            {
                if (spawner.Tick(Dt, rate, out _)) count++;
            }
            return count;
        }

        [Test]
        public void RateZero_NeverSpawns() => Assert.AreEqual(0, Count(Make(1), 0f, 600f));

        [Test]
        public void Rate_SpawnsAboutThatManyPerMinute()
        {
            int count = Count(Make(1), 12f, 600f);
            Assert.That(count, Is.InRange(80, 160));
        }

        [Test]
        public void SameSeed_SameSequence()
        {
            AmbientSpawner a = Make(7);
            AmbientSpawner b = Make(7);
            for (int i = 0; i < 2000; i++)
            {
                bool ra = a.Tick(Dt, 20f, out SpawnRequest qa);
                bool rb = b.Tick(Dt, 20f, out SpawnRequest qb);
                Assert.AreEqual(ra, rb);
                if (ra) Assert.AreEqual((qa.Lane, qa.LookIndex, qa.SpeedScale), (qb.Lane, qb.LookIndex, qb.SpeedScale));
            }
        }

        [Test]
        public void Requests_StayInRange_AndUseBothLanes()
        {
            AmbientSpawner spawner = Make(3);
            bool east = false;
            bool west = false;
            for (int i = 0; i < 20000; i++)
            {
                if (!spawner.Tick(Dt, 60f, out SpawnRequest request)) continue;
                Assert.That(request.LookIndex, Is.InRange(0, Looks - 1));
                Assert.That(request.SpeedScale, Is.InRange(0.8f, 1.2f));
                east |= request.Lane == TrafficLane.East;
                west |= request.Lane == TrafficLane.West;
            }
            Assert.IsTrue(east && west);
        }

        [Test]
        public void TurningRateOffThenOn_RestartsTheWait()
        {
            AmbientSpawner spawner = Make(1);
            Count(spawner, 0f, 10f);
            Assert.IsFalse(spawner.Tick(Dt, 6f, out _));
        }
    }
}
