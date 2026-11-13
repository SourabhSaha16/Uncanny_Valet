using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Events;
using UncannyValet.UI.Hud;

namespace UncannyValet.Tests.UI
{
    /// <summary>The freeze time look (fade, tracker, bubble maths) and its sounds.</summary>
    public class FreezeLookTests
    {
        [Test]
        public void Fade_RisesWhileFrozen_AndFallsAfterwards_InRealTime()
        {
            var fade = new FreezeFade(0.5f, 1f);
            fade.Frozen = true;
            fade.Tick(0.25f);
            Assert.AreEqual(0.5f, fade.Alpha01, 0.0001f);
            fade.Tick(5f);
            Assert.AreEqual(1f, fade.Alpha01);
            fade.Frozen = false;
            fade.Tick(0.5f);
            Assert.AreEqual(0.5f, fade.Alpha01, 0.0001f);
            fade.Tick(5f);
            Assert.AreEqual(0f, fade.Alpha01);
        }

        [Test]
        public void Fade_WithZeroFadeTime_JumpsAtOnce()
        {
            var fade = new FreezeFade(0f, 0f);
            fade.Frozen = true;
            fade.Tick(0.01f);
            Assert.AreEqual(1f, fade.Alpha01);
        }

        [Test]
        public void Tracker_FollowsFreezeTime_AndClearsOnANewShift()
        {
            var bus = new EventBus();
            var tracker = new FreezeTracker(bus, new FreezeFade(0.1f, 0.1f));
            tracker.Tick(0f);
            Assert.IsFalse(tracker.Fade.Frozen);
            bus.Publish(new PowerActivated(PowerKind.FreezeTime));
            tracker.Tick(1f);
            Assert.AreEqual(1f, tracker.Fade.Alpha01);
            bus.Publish(new PowerEnded(PowerKind.FreezeTime));
            Assert.IsFalse(tracker.Fade.Frozen);
            bus.Publish(new PowerActivated(PowerKind.FreezeTime));
            bus.Publish(new ShiftStarted());
            Assert.AreEqual(0f, tracker.Fade.Alpha01);
            Assert.IsFalse(tracker.Fade.Frozen);
            tracker.Dispose();
            bus.Publish(new PowerActivated(PowerKind.FreezeTime));
            Assert.IsFalse(tracker.Fade.Frozen, "no subscription left behind");
        }

        [Test]
        public void Bubble_IsWidestAtTheCentreRowAndClosesAtTheEdges()
        {
            Assert.AreEqual(40, FreezeLook.BubbleHalfWidth(0, 40, 34));
            Assert.Less(FreezeLook.BubbleHalfWidth(20, 40, 34), 40);
            Assert.AreEqual(0, FreezeLook.BubbleHalfWidth(34, 40, 34));
            Assert.AreEqual(0, FreezeLook.BubbleHalfWidth(-60, 40, 34));
            Assert.AreEqual(FreezeLook.BubbleHalfWidth(-12, 40, 34), FreezeLook.BubbleHalfWidth(12, 40, 34));
        }

        [Test]
        public void SpikeLengths_AreRepeatableAndInRange()
        {
            for (var edge = 0; edge < 4; edge++)
                for (var i = 0; i < 60; i++)
                {
                    var length = FreezeLook.SpikeLength(i, edge);
                    Assert.AreEqual(length, FreezeLook.SpikeLength(i, edge));
                    Assert.GreaterOrEqual(length, FreezeLook.SpikeMinLength);
                    Assert.LessOrEqual(length, FreezeLook.SpikeMinLength + FreezeLook.SpikeExtraLength);
                }
        }

        [Test]
        public void Router_PlaysTimeStopAndTimeResumeForFreezeTimeOnly()
        {
            var bus = new EventBus();
            var played = new List<SfxId>();
            var router = new SfxEventRouter(bus, played.Add);
            router.Subscribe();
            bus.Publish(new PowerActivated(PowerKind.FreezeTime));
            bus.Publish(new PowerEnded(PowerKind.FreezeTime));
            CollectionAssert.AreEqual(new[] { SfxId.TimeStop, SfxId.TimeResume }, played);
            router.Unsubscribe();
        }
    }
}
