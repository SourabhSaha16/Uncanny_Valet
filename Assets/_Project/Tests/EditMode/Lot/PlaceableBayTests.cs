using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>Bays, wash stations and the curb take their own size, position and rotation; the old signatures still give the config's size.</summary>
    public class PlaceableBayTests
    {
        private static readonly Vector2 BigBay = new Vector2(3.4f, 6f);

        private LotConfig _config;
        private GameObject _parent;
        private RectFactory _rects;
        private SpotOccupancy _occupancy;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _parent = new GameObject("Lot");
            _rects = new RectFactory();
            _occupancy = new SpotOccupancy(new List<int> { 1, 2 });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_parent);
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void RotatedBay_KeepsItsCentreRotationAndTriggerSize()
        {
            var spot = SpotBuilder.Build(_rects, _parent.transform, _config, 1, new Vector2(4f, -2f), _config.SpotSize, 90f, false, _occupancy);
            Assert.AreEqual(new Vector2(4f, -2f), (Vector2)spot.transform.localPosition);
            Assert.AreEqual(90f, spot.transform.localEulerAngles.z, 1e-3f);
            var box = spot.GetComponent<BoxCollider2D>();
            Assert.AreEqual(_config.SpotSize, box.size);
            Assert.IsTrue(box.isTrigger);
        }

        [Test]
        public void RotatedBay_CentreTestFollowsTheTurnedBox()
        {
            var spot = SpotBuilder.Build(_rects, _parent.transform, _config, 1, Vector2.zero, _config.SpotSize, 90f, false, _occupancy);
            spot.gameObject.SetActive(true);
            Physics2D.SyncTransforms();
            var box = spot.GetComponent<BoxCollider2D>();
            Assert.IsTrue(box.OverlapPoint(new Vector2(-2f, 0f)), "length now runs along x");
            Assert.IsFalse(box.OverlapPoint(new Vector2(0f, 2f)), "width now runs along y");
        }

        [Test]
        public void NonDefaultBay_UsesItsOwnSizeForTriggerAndInnerRectangle()
        {
            var spot = SpotBuilder.Build(_rects, _parent.transform, _config, 2, Vector2.zero, BigBay, 0f, false, _occupancy);
            Assert.AreEqual(BigBay, spot.GetComponent<BoxCollider2D>().size);
            Assert.AreEqual(BigBay - Vector2.one * 0.2f, SpotBuilder.InnerSize(BigBay));
            Assert.AreNotEqual(_config.SpotSize, BigBay);
        }

        [Test]
        public void OldBuildSignature_StillUsesTheConfigSpotSize()
        {
            var area = new Rect(5f, 6f, _config.SpotSize.x, _config.SpotSize.y);
            var spot = SpotBuilder.Build(_rects, _parent.transform, _config, 1, area, false, _occupancy);
            Assert.AreEqual(_config.SpotSize, spot.GetComponent<BoxCollider2D>().size);
            Assert.AreEqual(area.center, (Vector2)spot.transform.localPosition);
            Assert.AreEqual(0f, spot.transform.localEulerAngles.z, 1e-3f);
            Assert.AreEqual(_config.SpotSize - Vector2.one * 0.2f, SpotBuilder.InnerSize(_config));
        }

        [Test]
        public void WashStation_TakesSizeAndRotation_BlockerMatches()
        {
            var zone = StationBuilder.BuildWash(_rects, _parent.transform, _config, new Vector2(3f, 3f), BigBay, 180f);
            Assert.AreEqual(BigBay, zone.GetComponent<BoxCollider2D>().size);
            Assert.AreEqual(180f, zone.transform.localEulerAngles.z, 1e-3f);
            Assert.AreEqual(BigBay, zone.GetComponentInChildren<StationBlocker>(true).GetComponent<BoxCollider2D>().size);
        }

        [Test]
        public void CurbZone_TakesItsOwnSize_AndArrowsFollowIt()
        {
            var wide = new Vector2(6f, 5f);
            var go = new GameObject("Curb");
            go.transform.SetParent(_parent.transform, false);
            go.AddComponent<BoxCollider2D>();
            var curb = go.AddComponent<CurbZone>();
            var arrow = LotCurbArrows.Build(_rects, _parent.transform, _config, new Vector2(2f, 1f), wide);
            curb.Init(_config, wide, arrow);
            Assert.AreEqual(wide, go.GetComponent<BoxCollider2D>().size);
            Assert.IsTrue(curb.Span.IsMarked);
            Assert.IsFalse(LotCurbArrows.SpanOf(_config).Contains(1.5f, 0f));
            Assert.IsTrue(curb.Span.Contains(1.5f, 0f));
        }
    }
}
