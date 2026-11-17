using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Tests.Integration;
using UncannyValet.UI;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Pie;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class BayBorderTests
    {
        private sealed class FakeGeometry : ISpotGeometry
        {
            public bool TryGetSpot(int spotId, out SpotBounds bay)
            {
                bay = new SpotBounds(new Vector2(spotId, 0f), new Vector2(2f, 3f), 0f);
                return spotId > 0 && spotId < 10;
            }
        }

        private sealed class Rig
        {
            public readonly EventBus Bus = new EventBus();
            public readonly TransformCarFactory Cars = new TransformCarFactory();
            public readonly HudEmotionConfig Config = ScriptableObject.CreateInstance<HudEmotionConfig>();
            public readonly GameObject CarObject = new GameObject("car");
            public float Now;
            public readonly BayBorderLayer Layer;

            public Rig()
            {
                Cars.Add(new CarId(1), CarObject.transform);
                Layer = new BayBorderLayer(Bus, new FakeGeometry(), Cars, new EmotionStyle(Config), Config.BayBorder, Color.green, () => Now);
            }

            public void Dispose()
            {
                Layer.Dispose();
                Object.DestroyImmediate(CarObject);
                Object.DestroyImmediate(Config);
            }
        }

        private static TicketRow Waiting(float elapsed, int spot = 2)
        {
            var row = new TicketRow { Car = 1, Spot = spot, State = TicketRowState.PickupRequested };
            row.StartTimer(TimerKind.Pickup, elapsed);
            return row;
        }

        [Test]
        public void BlinkIsLitForTheFirstHalfOfEachPeriod()
        {
            Assert.IsTrue(BayBorderRule.IsLit(0f, 0f, 2f));
            Assert.IsTrue(BayBorderRule.IsLit(0.2f, 0f, 2f));
            Assert.IsFalse(BayBorderRule.IsLit(0.3f, 0f, 2f));
            Assert.IsTrue(BayBorderRule.IsLit(0.55f, 0f, 2f));
        }

        [Test]
        public void AngrierPatronsBlinkFaster_WithTheOldPlateSpeeds()
        {
            var s = new BayBorderSettings();
            Assert.AreEqual(1.5f, BayBorderRule.Hz(Emotion.Happy, s));
            Assert.AreEqual(3f, BayBorderRule.Hz(Emotion.Impatient, s));
            Assert.AreEqual(7f, BayBorderRule.Hz(Emotion.Angry, s));
        }

        [Test]
        public void TimerFlashOnlyWhileAPickupTimerRunsInABay()
        {
            Assert.IsTrue(BayBorderRule.TimerFlashes(Waiting(0.3f)));
            Assert.IsFalse(BayBorderRule.TimerFlashes(Waiting(0.3f, 0)), "no bay");
            var expired = Waiting(0.3f);
            expired.Expired = true;
            Assert.IsFalse(BayBorderRule.TimerFlashes(expired));
            var stopped = Waiting(0.3f);
            stopped.HasTimer = false;
            Assert.IsFalse(BayBorderRule.TimerFlashes(stopped));
        }

        [Test]
        public void ParkedFlashRunsForItsDurationOnly()
        {
            Assert.IsTrue(BayBorderRule.ParkedFlashRuns(1.5f, 1f, 0.8f));
            Assert.IsFalse(BayBorderRule.ParkedFlashRuns(1.9f, 1f, 0.8f));
        }

        [Test]
        public void GreenFlashStartsOnParkedInside_AndEndsAfterItsDuration()
        {
            var rig = new Rig();
            var rows = new List<TicketRow>();
            rig.Bus.Publish(new CarParkedInside(new CarId(1), 3));
            rig.Layer.Update(rows);
            Assert.AreEqual(1, rig.Layer.ActiveCount);
            rig.Now = rig.Config.BayBorder.ParkedFlashSeconds + 0.1f;
            rig.Layer.Update(rows);
            Assert.AreEqual(0, rig.Layer.ActiveCount);
            rig.Dispose();
        }

        [Test]
        public void GreenFlashStopsWhenTheCarLeavesTheBayOrTheShiftEnds()
        {
            var rig = new Rig();
            var rows = new List<TicketRow>();
            rig.Bus.Publish(new CarParkedInside(new CarId(1), 3));
            rig.Bus.Publish(new CarLeftSpot(new CarId(1), 3));
            rig.Layer.Update(rows);
            Assert.AreEqual(0, rig.Layer.ActiveCount, "left the bay");
            rig.Bus.Publish(new CarParkedInside(new CarId(1), 3));
            rig.Bus.Publish(new ShiftEnded());
            rig.Layer.Update(rows);
            Assert.AreEqual(0, rig.Layer.ActiveCount, "shift ended");
            rig.Dispose();
        }

        [Test]
        public void TimerFlashLastsWhileTheTimerRuns_ThenIsReleased()
        {
            var rig = new Rig();
            var rows = new List<TicketRow> { Waiting(0.2f) };
            rig.Layer.Update(rows);
            Assert.AreEqual(1, rig.Layer.ActiveCount);
            rows.Clear();
            rig.Layer.Update(rows);
            Assert.AreEqual(0, rig.Layer.ActiveCount, "ticket closed");
            rows.Add(Waiting(0.2f));
            rig.Layer.Update(rows);
            Assert.AreEqual(1, rig.Layer.ActiveCount);
            rows[0].Spot = 0;
            rig.Layer.Update(rows);
            Assert.AreEqual(0, rig.Layer.ActiveCount, "car left the bay");
            rig.Dispose();
        }

        [Test]
        public void ADestroyedCarEndsItsGreenFlash()
        {
            var rig = new Rig();
            var rows = new List<TicketRow>();
            rig.Bus.Publish(new CarParkedInside(new CarId(1), 3));
            Object.DestroyImmediate(rig.CarObject);
            rig.Layer.Update(rows);
            Assert.AreEqual(0, rig.Layer.ActiveCount);
            rig.Layer.Dispose();
            Object.DestroyImmediate(rig.Config);
        }

        [Test]
        public void ThePieIsTwentyPixelsByDefault()
        {
            Assert.AreEqual(20, new PickupPieSettings().DiameterPixels);
        }
    }
}
