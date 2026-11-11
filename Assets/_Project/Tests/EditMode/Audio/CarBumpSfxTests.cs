using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Events;

namespace UncannyValet.Tests.Audio
{
    public class CarBumpSfxTests
    {
        [Test]
        public void CarsCollided_PlaysTheBumpOnceAndIsRateLimitedWithAClock()
        {
            var bus = new EventBus();
            var played = new List<SfxId>();
            var now = 0f;
            var router = new SfxEventRouter(bus, played.Add, () => now);
            router.Subscribe();
            bus.Publish(new CarsCollided(1f, 2f, 0.5f));
            CollectionAssert.AreEqual(new[] { SfxId.CarBump }, played);
            now += SfxEventRouter.CarBumpMinInterval * 0.5f;
            bus.Publish(new CarsCollided(1f, 2f, 0.5f));
            Assert.AreEqual(1, played.Count);
            now += SfxEventRouter.CarBumpMinInterval;
            bus.Publish(new CarsCollided(1f, 2f, 0.5f));
            Assert.AreEqual(2, played.Count);
            router.Unsubscribe();
            bus.Publish(new CarsCollided(1f, 2f, 0.5f));
            Assert.AreEqual(2, played.Count);
        }

        [Test]
        public void CarsCollided_WithoutAClockPlaysEveryEvent()
        {
            var bus = new EventBus();
            var played = new List<SfxId>();
            new SfxEventRouter(bus, played.Add).Subscribe();
            bus.Publish(new CarsCollided(0f, 0f, 1f));
            bus.Publish(new CarsCollided(0f, 0f, 1f));
            Assert.AreEqual(2, played.Count);
        }

        [Test]
        public void CarBumpRecipe_IsShortAudibleAndWithinRange()
        {
            var samples = SfxRecipeBook.Render(SfxId.CarBump, 22050);
            Assert.Greater(samples.Length, 22050 / 10);
            Assert.Less(samples.Length, 22050);
            var peak = 0f;
            foreach (var s in samples) peak = System.Math.Max(peak, System.Math.Abs(s));
            Assert.Greater(peak, 0.1f);
            Assert.LessOrEqual(peak, 1f);
        }
    }
}
