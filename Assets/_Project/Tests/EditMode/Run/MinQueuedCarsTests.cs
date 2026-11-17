using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Tests.Run
{
    /// <summary>Issue 178: the queue minimum is authored per lot; only Vanaheim training has one (2).</summary>
    public class MinQueuedCarsTests
    {
        private const int VanaheimTrainingMinimum = 2;

        private static float Seeded(string path)
        {
            var tuning = VanaheimAssets.Seeded(VanaheimAssets.LotAt(path));
            return tuning.TryGet(TuningKey.PlanMinQueuedCars, out var v) ? v : -1f;
        }

        [Test]
        public void VanaheimTrainingKeepsTwoCarsQueued()
        {
            Assert.AreEqual(VanaheimTrainingMinimum, VanaheimAssets.LotAt(VanaheimAssets.TrainingPath).Pacing.MinQueuedCars);
            Assert.AreEqual(VanaheimTrainingMinimum, Seeded(VanaheimAssets.TrainingPath));
        }

        [Test]
        public void VanaheimTrainingMinimumFitsTheWaitingCap()
        {
            var pacing = VanaheimAssets.LotAt(VanaheimAssets.TrainingPath).Pacing;
            Assert.LessOrEqual(pacing.MinQueuedCars, pacing.MaxWaitingCars);
        }

        [Test]
        public void OtherLotsHaveNoMinimum()
        {
            Assert.AreEqual(0, VanaheimAssets.LotAt(VanaheimAssets.CrazyPath).Pacing.MinQueuedCars);
            Assert.AreEqual(0f, Seeded(VanaheimAssets.CrazyPath));
        }
    }
}
