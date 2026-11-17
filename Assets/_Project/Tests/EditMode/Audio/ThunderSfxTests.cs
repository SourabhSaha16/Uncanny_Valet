using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Audio
{
    public class ThunderSfxTests
    {
        [Test]
        public void ThunderCrackKeepsItsNumber_AndPlaysOnlyForThunderStride()
        {
            Assert.AreEqual(25, (int)SfxId.ThunderCrack, "SfxId numbers are append-only");
            var bus = new EventBus();
            var played = new List<SfxId>();
            new SfxEventRouter(bus, played.Add).Subscribe();
            bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            bus.Publish(new PowerActivated(StoryPower.StormWind, 1));
            bus.Publish(new PowerEnded(StoryPower.ThunderStride));
            CollectionAssert.AreEqual(new[] { SfxId.ThunderCrack }, played);
        }
    }
}
