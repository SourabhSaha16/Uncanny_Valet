using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Tests.Integration;
using UncannyValet.UI;
using UncannyValet.UI.Mood;
using UncannyValet.UI.Pie;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class PickupPieTests
    {
        private const int Steps = 24;

        private static TicketRow Running(float elapsed, int car = 1)
        {
            var row = new TicketRow { Car = car, State = TicketRowState.PickupRequested };
            row.StartTimer(TimerKind.Pickup, elapsed);
            return row;
        }

        [Test]
        public void FullAtStart_EmptyAtEnd_CutGrowsClockwise()
        {
            Assert.AreEqual(0f, PickupPieRule.CutDegrees(0f));
            Assert.AreEqual(90f, PickupPieRule.CutDegrees(0.25f), 1e-3f);
            Assert.AreEqual(360f, PickupPieRule.CutDegrees(1f));
            Assert.AreEqual(360f, PickupPieRule.CutDegrees(3f), "clamped");
            Assert.AreEqual(0, PickupPieRule.Step(0f, Steps));
            Assert.AreEqual(Steps / 2, PickupPieRule.Step(0.5f, Steps));
            Assert.AreEqual(Steps, PickupPieRule.Step(1f, Steps));
        }

        [Test]
        public void ShownOnlyWhileAPickupTimerRuns()
        {
            Assert.IsTrue(PickupPieRule.IsShown(Running(0f), Steps));
            Assert.IsTrue(PickupPieRule.IsShown(Running(0.9f), Steps));
            Assert.IsFalse(PickupPieRule.IsShown(Running(1f), Steps), "worn off");
            var expired = Running(0.5f);
            expired.Expired = true;
            Assert.IsFalse(PickupPieRule.IsShown(expired, Steps));
            var stopped = Running(0.5f);
            stopped.HasTimer = false;
            Assert.IsFalse(PickupPieRule.IsShown(stopped, Steps));
            var parked = Running(0.5f);
            parked.State = TicketRowState.Parked;
            Assert.IsFalse(PickupPieRule.IsShown(parked, Steps));
        }

        [Test]
        public void ColourStateFollowsTheConfiguredPatienceLevels()
        {
            var config = ScriptableObject.CreateInstance<HudEmotionConfig>();
            var rule = new EmotionStyle(config).Rule;
            Assert.AreEqual(Emotion.Happy, PickupPieRule.ColorState(Running(config.ImpatientFrom - 0.01f), rule));
            Assert.AreEqual(Emotion.Impatient, PickupPieRule.ColorState(Running(config.ImpatientFrom), rule));
            Assert.AreEqual(Emotion.Angry, PickupPieRule.ColorState(Running(config.AngryFrom), rule));
            Object.DestroyImmediate(config);
        }

        [Test]
        public void ClockwiseFractionRunsFromTwelveThroughThreeAndSix()
        {
            const int size = 16;
            Assert.Less(PiePixels.ClockwiseFraction(8, 15, size), 0.05f, "just right of 12 o'clock");
            Assert.AreEqual(0.25f, PiePixels.ClockwiseFraction(15, 8, size), 0.05f, "3 o'clock");
            Assert.AreEqual(0.5f, PiePixels.ClockwiseFraction(7, 0, size), 0.05f, "6 o'clock");
            Assert.AreEqual(0.75f, PiePixels.ClockwiseFraction(0, 7, size), 0.05f, "9 o'clock");
            Assert.Greater(PiePixels.ClockwiseFraction(7, 15, size), 0.95f, "just left of 12 o'clock");
        }

        [Test]
        public void CutWedgeRemovesTheStartSideFirst()
        {
            const int size = 16;
            Assert.IsTrue(PiePixels.InWedge(12, 12, size, 0f), "full pie");
            Assert.IsFalse(PiePixels.InWedge(12, 12, size, 0.25f), "upper right quarter cut");
            Assert.IsTrue(PiePixels.InWedge(4, 4, size, 0.25f), "lower left stays");
            Assert.IsFalse(PiePixels.InWedge(4, 4, size, 1f), "nothing left");
            Assert.IsFalse(PiePixels.InWedge(0, 0, size, 0f), "corner is outside the disc");
        }

        [Test]
        public void SpritesAreBuiltOncePerStep_WithNoneForTheEmptyPie()
        {
            var sprites = new PickupPieSprites(14, Steps);
            Assert.IsNotNull(sprites.Back);
            Assert.IsNotNull(sprites.Wedge(0));
            Assert.IsNotNull(sprites.Wedge(Steps - 1));
            Assert.IsNull(sprites.Wedge(Steps));
            Assert.AreSame(sprites.Wedge(3), sprites.Wedge(3));
            Assert.AreEqual(32f, sprites.Back.pixelsPerUnit);
            sprites.Dispose();
        }

        [Test]
        public void LayerShowsAPieOnlyWhileTheTimerRuns_AndReleasesItAfterwards()
        {
            var config = ScriptableObject.CreateInstance<HudEmotionConfig>();
            var cars = new TransformCarFactory();
            var carObject = new GameObject("car");
            carObject.transform.position = new Vector3(2f, 3f, 0f);
            cars.Add(new CarId(1), carObject.transform);
            var settings = new PickupPieSettings();
            var layer = new PickupPieLayer(cars, new EmotionStyle(config), settings);
            var rows = new List<TicketRow> { Running(0.2f) };

            layer.Update(rows);
            Assert.AreEqual(1, layer.ActiveCount);
            rows[0].HasTimer = false;
            layer.Update(rows);
            Assert.AreEqual(0, layer.ActiveCount, "timer stopped");
            rows[0].StartTimer(TimerKind.Pickup, 0.4f);
            layer.Update(rows);
            Assert.AreEqual(1, layer.ActiveCount);
            rows.Clear();
            layer.Update(rows);
            Assert.AreEqual(0, layer.ActiveCount, "ticket closed");

            rows.Add(Running(0.4f));
            layer.Update(rows);
            Assert.AreEqual(1, layer.ActiveCount);
            Object.DestroyImmediate(carObject);
            layer.Update(rows);
            Assert.AreEqual(0, layer.ActiveCount, "car destroyed");

            layer.Dispose();
            Object.DestroyImmediate(config);
        }
    }
}
