using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class WalkwayLayoutTests
    {
        private LotConfig _config;
        private WalkwayStyle _style;
        private DecorLayout _decor;
        private WalkwayLayout _layout;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _style = new WalkwayStyle();
            var lot = new LotGeometry(_config).Bounds;
            _decor = new DecorLayout(lot, _config.WallThickness, 1.5f, 3f, 8f, 40f);
            _layout = new WalkwayLayout(_config, _decor, _style, 5f);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void EachAisleHasTwoCrossingsAndTwoRoutesAcrossIt()
        {
            var aisles = _config.Rows - 1;
            Assert.AreEqual(aisles * 2, _layout.Crossings.Length);
            Assert.AreEqual(aisles * 2 + 3, _layout.Routes.Length); // plus two road routes and the north sidewalk
        }

        [Test]
        public void CrossingsSpanTheAisleExactly()
        {
            var aisle = _layout.Crossings[0];
            Assert.AreEqual(_config.AisleWidth, aisle.height, 1e-4f);
            Assert.AreEqual(_style.CrossingWidth, aisle.width, 1e-4f);
            Assert.AreEqual(0f, aisle.center.y, 1e-4f, "two rows are centred on the lot origin");
        }

        [Test]
        public void CrossingsDoNotOverlapEachOther()
        {
            Assert.IsFalse(_layout.Crossings[0].Overlaps(_layout.Crossings[1]));
        }

        [Test]
        public void AisleRoutesCrossTheWholeAisle()
        {
            var route = _layout.Routes[0];
            Assert.AreEqual(2, route.Length);
            Assert.Greater(route[0].y, _layout.Crossings[0].yMax);
            Assert.Less(route[1].y, _layout.Crossings[0].yMin);
            Assert.AreEqual(route[0].x, route[1].x, 1e-4f);
            Assert.AreEqual(_layout.Crossings[0].center.x, route[0].x, 1e-4f);
        }

        [Test]
        public void RoadRoutesCrossAtTheRoadCrosswalk()
        {
            var crossX = _config.CurbPosition.x + 5f;
            var route = _layout.Routes[_layout.Routes.Length - 3];
            Assert.AreEqual(crossX, route[1].x, 1e-4f);
            Assert.AreEqual(crossX, route[2].x, 1e-4f);
            Assert.AreEqual(_decor.SouthSidewalk.center.y, route[1].y, 1e-4f);
            Assert.AreEqual(_decor.FarSidewalk.center.y, route[2].y, 1e-4f);
        }

        [Test]
        public void NorthRouteWalksTheNorthSidewalk()
        {
            var route = _layout.Routes[_layout.Routes.Length - 1];
            Assert.AreEqual(_decor.NorthSidewalk.center.y, route[0].y, 1e-4f);
            Assert.AreEqual(_decor.NorthSidewalk.center.y, route[1].y, 1e-4f);
            Assert.Less(route[0].x, route[1].x);
        }

        [Test]
        public void ArrowsSitInTheAisleAndPointOppositeWaysInTheTwoLanes()
        {
            Assert.AreEqual((_config.Rows - 1) * 2, _layout.Arrows.Length);
            var east = _layout.Arrows[0];
            var west = _layout.Arrows[1];
            Assert.Greater(east.Tip.x, east.Tail.x);
            Assert.Less(west.Tip.x, west.Tail.x);
            Assert.Greater(east.Tail.y, west.Tail.y);
            Assert.Less(Mathf.Abs(east.Tail.y), _config.AisleWidth * 0.5f);
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
