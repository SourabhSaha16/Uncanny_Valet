using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class LotExpansionTests
    {
        private LotGeometry _geo;
        private SpotOccupancy _occ;

        [SetUp]
        public void SetUp()
        {
            _geo = new LotGeometry(ScriptableObject.CreateInstance<LotConfig>());
            var ids = new List<int>();
            for (var i = 1; i <= _geo.Spots.Length + _geo.ChargingSpots.Length; i++) ids.Add(i);
            _occ = new SpotOccupancy(ids, _geo.FirstChargingId);
        }

        [Test]
        public void ChargingSpots_AreClosedUntilChargingOpens()
        {
            var charge = _geo.FirstChargingId;
            Assert.IsFalse(_occ.IsFree(charge));
            Assert.IsFalse(_occ.Park(charge, new CarId(1)));
            _occ.SetChargingOpen(true);
            Assert.IsTrue(_occ.IsFree(charge));
            Assert.IsTrue(_occ.Park(charge, new CarId(1)));
        }

        [Test]
        public void ChargingSpotWithACar_StaysOpenUntilItLeaves()
        {
            var charge = _geo.FirstChargingId;
            _occ.SetChargingOpen(true);
            _occ.Park(charge, new CarId(1));
            _occ.SetChargingOpen(false);
            Assert.IsTrue(_occ.IsOpen(charge));
            _occ.Leave(charge, new CarId(1));
            Assert.IsFalse(_occ.IsOpen(charge));
        }

        [Test]
        public void ActiveSpots_DoNotCountChargingSpots()
        {
            _occ.SetActiveSpots(7);
            _occ.SetChargingOpen(true);
            Assert.IsTrue(_occ.IsOpen(7));
            Assert.IsFalse(_occ.IsOpen(8));
            Assert.IsTrue(_occ.IsOpen(_geo.FirstChargingId + 1));
        }

        [Test]
        public void OccupiedSpotBeyondTheLimit_KeepsItsColumnInTheFrame()
        {
            var last = _geo.Spots.Length;
            _occ.Park(last, new CarId(1));
            _occ.SetActiveSpots(4);
            Assert.AreEqual(_geo.FrameFor(6), LotFrameRule.Frame(_geo, _occ));
            _occ.Leave(last, new CarId(1));
            Assert.AreEqual(_geo.FrameFor(4), LotFrameRule.Frame(_geo, _occ));
        }

        [Test]
        public void Frame_FollowsOpenRegularSpots_TheStationsSitInsideIt()
        {
            _occ.SetActiveSpots(7);
            Assert.AreEqual(_geo.FrameFor(5), LotFrameRule.Frame(_geo, _occ));
            _occ.SetChargingOpen(true);
            Assert.AreEqual(_geo.FrameFor(5), LotFrameRule.Frame(_geo, _occ), "opening the stations does not move the walls");
            _occ.SetActiveSpots(0);
            Assert.AreEqual(_geo.Bounds, LotFrameRule.Frame(_geo, _occ));
        }

        [Test]
        public void FewRegularSpotsOpen_StillFrameTheTwoStationColumns()
        {
            _occ.SetActiveSpots(1);
            var frame = _geo.FrameFor(2);
            Assert.AreEqual(frame, LotFrameRule.Frame(_geo, _occ));
            Assert.IsTrue(frame.Contains(_geo.Wash.center));
            Assert.IsTrue(frame.Contains(_geo.ChargingSpots[1].center));
        }
    }
}
