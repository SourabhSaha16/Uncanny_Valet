using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Tests.Audio
{
    /// <summary>The comic yelp that replaces the boing for a pedestrian a car hit.</summary>
    public class PedestrianYelpTests
    {
        [Test]
        public void YelpKeepsItsNumberAndRendersASoftSound()
        {
            Assert.AreEqual(26, (int)SfxId.PedestrianYelp, "SfxId numbers are append-only");
            var samples = SfxRecipeBook.Render(SfxId.PedestrianYelp, 22050);
            Assert.Greater(samples.Length, 0);
            var peak = 0f;
            foreach (var s in samples) peak = Mathf.Max(peak, Mathf.Abs(s));
            Assert.Greater(peak, 0.05f);
            Assert.LessOrEqual(peak, 1f);
        }

        [Test]
        public void ARouterPlaysTheYelpOncePerHitAndNotTheBoing()
        {
            var bus = new EventBus();
            var played = new System.Collections.Generic.List<SfxId>();
            var now = 0f;
            var router = new SfxEventRouter(bus, played.Add, () => now);
            router.Subscribe();
            var hit = new PedestrianHit(new PedestrianId(1), true, Vector2.zero, 5f, true, new CarId(1));
            bus.Publish(hit);
            bus.Publish(hit);
            CollectionAssert.AreEqual(new[] { SfxId.PedestrianYelp }, played);
            now += SfxEventRouter.YelpMinInterval + 0.01f;
            bus.Publish(hit);
            Assert.AreEqual(2, played.Count);
            router.Unsubscribe();
        }
    }
}
