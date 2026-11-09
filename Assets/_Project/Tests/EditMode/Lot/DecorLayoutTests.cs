using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class DecorLayoutTests
    {
        private static readonly Rect LotRect = Rect.MinMaxRect(-19f, -9f, 10f, 9f);
        private DecorLayout _layout;

        [SetUp]
        public void SetUp()
        {
            _layout = new DecorLayout(LotRect, 0.6f, 1.5f, 3f, 8f, 40f);
        }

        [Test]
        public void SouthStackIsWallSidewalkRoadSidewalk()
        {
            Assert.AreEqual(LotRect.yMin - 0.6f, _layout.SouthSidewalk.yMax, 1e-4f);
            Assert.AreEqual(_layout.SouthSidewalk.yMin, _layout.Road.yMax, 1e-4f);
            Assert.AreEqual(_layout.Road.yMin, _layout.FarSidewalk.yMax, 1e-4f);
            Assert.AreEqual(3f, _layout.Road.height, 1e-4f);
        }

        [Test]
        public void RoadReachesPastTheLotOnBothSides()
        {
            Assert.AreEqual(LotRect.xMin - 8f, _layout.Road.xMin, 1e-4f);
            Assert.AreEqual(LotRect.xMax + 8f, _layout.Road.xMax, 1e-4f);
        }

        [Test]
        public void DinerStandsOnTheNorthSidewalkAboveTheLot()
        {
            Assert.AreEqual(_layout.NorthSidewalk.yMax, _layout.BuildingBase.y, 1e-4f);
            Assert.Greater(_layout.NorthSidewalk.yMin, LotRect.yMax);
        }

        [Test]
        public void TreesAndTuftsNeverStandInsideTheLot()
        {
            foreach (var tree in _layout.TreeBases()) Assert.IsFalse(LotRect.Contains(tree), "tree " + tree);
            foreach (var tuft in _layout.TuftSpots(60)) Assert.IsFalse(LotRect.Contains(tuft), "tuft " + tuft);
        }

        [Test]
        public void TuftsAreDeterministicAndCounted()
        {
            var a = _layout.TuftSpots(10);
            var b = _layout.TuftSpots(10);
            Assert.AreEqual(10, a.Length);
            Assert.AreEqual(a, b);
            Assert.AreEqual(0, _layout.TuftSpots(0).Length);
        }
    }
}
