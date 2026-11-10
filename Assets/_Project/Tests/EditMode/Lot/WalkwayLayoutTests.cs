using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class WalkwayLayoutTests
    {
        private const float Width = 1.4f;
        private LotConfig _config;
        private LotGeometry _geo;
        private WalkwayLayout _layout;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _geo = new LotGeometry(_config);
            _layout = new WalkwayLayout(_geo, Width);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void FourBandsMakeTheCrosswalkNetwork()
        {
            Assert.AreEqual(4, _layout.Bands.Length);
        }

        [Test]
        public void SideDoorLegRunsFromTheSideDoorEastToTheNorthCrosswalk()
        {
            var leg = _layout.Bands[0];
            Assert.AreEqual(_geo.Diner.xMax, leg.xMin, 1e-4f);
            Assert.AreEqual(_geo.SideDoor.center.y, leg.center.y, 1e-4f);
            Assert.AreEqual(Width, leg.height, 1e-4f);
            Assert.GreaterOrEqual(leg.xMax, _geo.CrossingX);
        }

        [Test]
        public void NorthCrosswalkRunsFromTheSideDoorLegSouthToTheAisle()
        {
            var band = _layout.Bands[1];
            Assert.AreEqual(_geo.CrossingX, band.center.x, 1e-4f);
            Assert.AreEqual(Width, band.width, 1e-4f);
            Assert.GreaterOrEqual(band.yMax, _geo.SideDoor.center.y);
            Assert.LessOrEqual(band.yMin, _geo.AisleY);
        }

        [Test]
        public void AisleCrossingRunsAlongTheAisleToTheRightStrip()
        {
            var band = _layout.Bands[2];
            Assert.AreEqual(_geo.AisleY, band.center.y, 1e-4f);
            Assert.AreEqual(_geo.Strip.xMin, band.xMax, 1e-4f);
            Assert.LessOrEqual(band.xMin, _geo.CrossingX);
        }

        [Test]
        public void NoBandCoversABay()
        {
            foreach (var band in _layout.Bands)
            {
                foreach (var spot in _geo.Spots) Assert.IsFalse(band.Overlaps(spot), "band over a spot");
                foreach (var bay in _geo.ChargingSpots) Assert.IsFalse(band.Overlaps(bay), "band over the EV bay");
                Assert.IsFalse(band.Overlaps(_geo.Wash), "band over the wash");
            }
        }

        [Test]
        public void SouthCrossingRunsDownBesideTheQueueColumn()
        {
            var band = _layout.Bands[3];
            Assert.AreEqual(_geo.CrossingX, band.center.x, 1e-4f);
            Assert.AreEqual(_geo.Bounds.yMin, band.yMin, 1e-4f);
            foreach (var car in _geo.QueueCars) Assert.IsFalse(band.Overlaps(car), "band over a queue car");
        }

        [Test]
        public void ArrowHeadStrokesFoldBackFromTheTip()
        {
            var arrow = new PaintedArrow(Vector2.zero, new Vector2(2f, 0f));
            var a = arrow.HeadEnd(1f, 45f);
            var b = arrow.HeadEnd(1f, -45f);
            Assert.Less(a.x, 2f);
            Assert.AreEqual(a.x, b.x, 1e-4f);
            Assert.AreEqual(-a.y, b.y, 1e-4f);
            Assert.AreEqual(1f, (a - arrow.Tip).magnitude, 1e-4f);
        }
    }
}
