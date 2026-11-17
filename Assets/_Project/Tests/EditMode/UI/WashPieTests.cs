using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Tests.Integration;
using UncannyValet.UI;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Pie;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class WashPieTests
    {
        private const int Steps = 24;
        private const float Gap = 0.8f;

        private static TicketRow Muddy(bool inWash, float progress, int car = 1)
        {
            var row = new TicketRow { Car = car, Arrived = true }.Give(false, true);
            row.Find(ModifierId.Muddy).Progress01 = progress;
            row.HasStation = inWash;
            row.Station = StationKind.Wash;
            return row;
        }

        [Test]
        public void ShownOnlyWhileAMuddyCarSitsInTheWash()
        {
            Assert.IsTrue(WashPieRule.IsShown(Muddy(true, 0f), Steps));
            Assert.IsTrue(WashPieRule.IsShown(Muddy(true, 0.9f), Steps));
            Assert.IsFalse(WashPieRule.IsShown(Muddy(false, 0.3f), Steps), "not in the wash");
            Assert.IsFalse(WashPieRule.IsShown(Muddy(true, 1f), Steps), "wash done");
            var cleared = Muddy(true, 0.5f);
            cleared.Clear(ModifierId.Muddy);
            Assert.IsFalse(WashPieRule.IsShown(cleared, Steps), "already washed");
            var charging = Muddy(true, 0.5f);
            charging.Station = StationKind.Charging;
            Assert.IsFalse(WashPieRule.IsShown(charging, Steps), "another station");
        }

        [Test]
        public void WedgeDrainsAsTheWashProgresses()
        {
            Assert.AreEqual(0, WashPieRule.Step(0f, Steps));
            Assert.AreEqual(Steps / 2, WashPieRule.Step(0.5f, Steps));
            Assert.AreEqual(Steps, WashPieRule.Step(1f, Steps));
        }

        [Test]
        public void TwoPies_SitSideBySideWithoutOverlap_OnePieStaysCentred()
        {
            var both = Muddy(true, 0.2f);
            both.StartTimer(TimerKind.Pickup, 0.3f);
            both.State = TicketRowState.PickupRequested;
            var left = PieSlots.PickupShift(both, Steps, Gap);
            var right = PieSlots.WashShift(both, Steps, Gap);
            Assert.AreEqual(Gap, right - left, 1e-4f);
            Assert.Less(left, 0f);
            Assert.Greater(right, 0f);
            var diameterUnits = new PickupPieSettings().DiameterPixels / PickupPieSprites.PixelsPerUnit;
            Assert.Greater(right - left, diameterUnits, "default gap clears the pie diameter");

            var washOnly = Muddy(true, 0.2f);
            Assert.AreEqual(0f, PieSlots.WashShift(washOnly, Steps, Gap));
            Assert.AreEqual(0f, PieSlots.PickupShift(washOnly, Steps, Gap));
        }

        [Test]
        public void LayerShowsAWashPieOnlyWhileWashing_AndReleasesItAfterwards()
        {
            var cars = new TransformCarFactory();
            var carObject = new GameObject("car");
            cars.Add(new CarId(1), carObject.transform);
            var layer = new WashPieLayer(cars, new PickupPieSettings());
            var rows = new List<TicketRow> { Muddy(true, 0.1f) };

            layer.Update(rows);
            Assert.AreEqual(1, layer.ActiveCount);
            rows[0].HasStation = false;
            layer.Update(rows);
            Assert.AreEqual(0, layer.ActiveCount, "car left the wash");
            rows[0].HasStation = true;
            layer.Update(rows);
            Assert.AreEqual(1, layer.ActiveCount);
            rows[0].Clear(ModifierId.Muddy);
            layer.Update(rows);
            Assert.AreEqual(0, layer.ActiveCount, "washed: the drop takes over");

            rows[0] = Muddy(true, 0.4f);
            layer.Update(rows);
            Object.DestroyImmediate(carObject);
            layer.Update(rows);
            Assert.AreEqual(0, layer.ActiveCount, "car destroyed");
            layer.Dispose();
        }

        [Test]
        public void TheDropIconHasABitmapWithKnownKeys()
        {
            Assert.AreEqual(9, PixelIconData.Rows(PixelIcon.Drop).Length);
            foreach (var row in PixelIconData.Rows(PixelIcon.Drop))
                foreach (var key in row)
                    Assert.IsTrue(key == '.' || PixelIconData.Keys.IndexOf(key) >= 0, "unknown icon key " + key);
            Assert.AreEqual(PixelIconData.Keys.Length, PixelIconData.Colors.Length);
        }
    }
}
