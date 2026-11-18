using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Tests.Integration;
using UncannyValet.UI;
using UncannyValet.UI.Hud;
using UncannyValet.UI.Kit;
using UncannyValet.UI.Pie;
using UnityEngine;

namespace UncannyValet.Tests.UI
{
    public class WashPieTests
    {
        private const int Steps = 24;

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
        public void WashPie_SitsAtTheDropletSpotBelowTheCar_NotWherePickupPieSits()
        {
            Assert.Greater(WorldTicketLayout.IconCentreDropWorldUnits, WorldTicketLayout.IconDropWorldUnits);
            Assert.Greater(new PickupPieSettings().Offset.y, 0f, "pickup pie stays above the car");
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

        [TestCase(PixelIcon.Drop)]
        [TestCase(PixelIcon.ThumbsUp)]
        public void TheStatusIconsHaveABitmapWithKnownKeys(PixelIcon icon)
        {
            Assert.AreEqual(9, PixelIconData.Rows(icon).Length);
            foreach (var row in PixelIconData.Rows(icon))
            {
                Assert.AreEqual(9, row.Length);
                foreach (var key in row)
                    Assert.IsTrue(key == '.' || PixelIconData.Keys.IndexOf(key) >= 0, "unknown icon key " + key);
            }
            Assert.AreEqual(PixelIconData.Keys.Length, PixelIconData.Colors.Length);
        }
    }
}
