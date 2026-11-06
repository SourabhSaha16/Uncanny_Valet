using System;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Audio
{
    public class SfxTests
    {
        private const int Rate = 22050;

        [Test]
        public void EveryEffectIsShortAudibleAndWithinFullScale()
        {
            foreach (SfxId id in Enum.GetValues(typeof(SfxId)))
            {
                float[] clip = SfxRecipeBook.Render(id, Rate);
                Assert.LessOrEqual(clip.Length, Rate, id.ToString());
                float peak = 0f;
                foreach (float s in clip) peak = Math.Max(peak, Math.Abs(s));
                Assert.Greater(peak, 0.1f, id.ToString());
                Assert.LessOrEqual(peak, 1f, id.ToString());
            }
        }

        [Test]
        public void RouterPlaysMatchingEffectsAndStopsAfterUnsubscribe()
        {
            var bus = new EventBus();
            var played = new System.Collections.Generic.List<SfxId>();
            var router = new SfxEventRouter(bus, played.Add);
            router.Subscribe();
            router.Subscribe();
            bus.Publish(new CarEntered(new CarId(1)));
            bus.Publish(new TipEarned(new TicketId(1), 5));
            bus.Publish(new NightEnded(1));
            bus.Publish(new NextNightRequested(1));
            CollectionAssert.AreEqual(
                new[] { SfxId.CarEnter, SfxId.TipEarned, SfxId.NightEnded, SfxId.ButtonClick }, played);

            router.Unsubscribe();
            bus.Publish(new CarExited(new CarId(1)));
            bus.Publish(new NextNightRequested(2));
            Assert.AreEqual(4, played.Count);
        }
    }
}
