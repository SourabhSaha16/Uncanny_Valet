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
            bus.Publish(new ShiftEnded());
            bus.Publish(new RetryShiftRequested());
            bus.Publish(new CarFullyCharged(new CarId(1)));
            bus.Publish(new CarCleaned(new CarId(1)));
            bus.Publish(new CarEnteredStation(new CarId(1), StationKind.Wash));
            CollectionAssert.AreEqual(new[]
            {
                SfxId.CarEnter, SfxId.TipEarned, SfxId.NightEnded, SfxId.ButtonClick,
                SfxId.Charged, SfxId.Washed, SfxId.StationStart
            }, played);

            router.Unsubscribe();
            bus.Publish(new CarExited(new CarId(1)));
            bus.Publish(new CarCleaned(new CarId(1)));
            Assert.AreEqual(7, played.Count);
        }
    }
}
