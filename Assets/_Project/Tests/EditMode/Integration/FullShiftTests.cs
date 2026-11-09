using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Integration
{
    public class FullShiftTests
    {
        private ShiftHarness _h;

        [SetUp] public void SetUp() { _h = new ShiftHarness(); }
        [TearDown] public void TearDown() { _h.Dispose(); }

        // Tip config defaults: window 10 min, bonus +2, penalty 0.5 per minute late. The harness has no patience bonus.
        [Test]
        public void WholeShift_TicketToTipToScoreToResult()
        {
            _h.StartShift();
            var ticket = _h.Spawn(new PatronRequest(30f, 20, false));
            _h.Minutes(5);
            Assert.IsTrue(_h.Park(ticket, 3));
            Assert.AreEqual(TicketState.Parked, ticket.State);

            _h.Minutes(30);
            Assert.AreEqual(TicketState.PickupRequested, ticket.State);
            Assert.AreEqual(1, _h.Log.Count<PickupRequested>());

            _h.Minutes(4);
            _h.AtCurb(ticket);

            Assert.AreEqual(TicketState.Closed, ticket.State);
            Assert.AreEqual(4f, _h.Log.Last<TicketClosed>().WaitMinutes);
            Assert.AreEqual(22, _h.Log.Last<TicketClosed>().Tip);
            Assert.AreEqual(22, _h.Log.Last<TipEarned>().Amount);
            Assert.AreEqual(22, _h.Score.Score);
            Assert.AreEqual(2, _h.Score.ComboMultiplier, "a happy patron raises the combo");
            Assert.AreEqual(0, _h.Registry.OpenCount);
            Assert.AreEqual(1, _h.Cars.DespawnCount);

            _h.Minutes(400);
            Assert.AreEqual(1, _h.Log.Count<ShiftEnded>());
            Assert.AreEqual(1, _h.Log.Count<ShiftResultReady>());
            var result = _h.Log.Last<ShiftResultReady>().Result;
            Assert.AreEqual(22, result.Score);
            Assert.AreEqual(0, result.Stars);
            Assert.AreEqual(0, result.Complaints);
        }

        [Test]
        public void EventOrder_FollowsTheLoop()
        {
            _h.StartShift();
            var t = _h.Spawn(new PatronRequest(10f, 20, false));
            _h.Minutes(1);
            _h.Park(t, 1);
            _h.Minutes(10);
            _h.AtCurb(t);
            var l = _h.Log;
            Assert.Less(l.IndexOf<ShiftStarted>(), l.IndexOf<TicketOpened>());
            Assert.Less(l.IndexOf<TicketOpened>(), l.IndexOf<CarParked>());
            Assert.Less(l.IndexOf<CarParked>(), l.IndexOf<PickupRequested>());
            Assert.Less(l.IndexOf<PickupRequested>(), l.IndexOf<TicketClosed>());
            Assert.Less(l.IndexOf<TicketClosed>(), l.IndexOf<TipEarned>());
            Assert.AreEqual(22, l.Last<ScoreChanged>().Score, "score reacts to the tip");
            Assert.Less(l.IndexOf<TicketClosed>(), l.IndexOf<CarDeparted>());
        }

        [Test]
        public void VeryLateReturn_TipIsZero_AndTheAngryPatronCostsAComplaint()
        {
            _h.StartShift();
            var t = _h.Spawn(new PatronRequest(30f, 20, false));
            _h.Minutes(5);
            _h.Park(t, 1);
            _h.Minutes(30 + 100);
            _h.AtCurb(t);
            Assert.AreEqual(0, _h.Log.Last<TipEarned>().Amount);
            Assert.AreEqual(1, _h.Log.Count<ComplaintFiled>());
            Assert.AreEqual(15, _h.Log.Last<ComplaintFiled>().Penalty);
            Assert.AreEqual(1, _h.Score.Complaints);
            Assert.AreEqual(0, _h.Score.Score, "the score never drops below 0");
            Assert.AreEqual(1, _h.Score.ComboMultiplier);
        }

        [Test]
        public void ImpatientPatron_ReturnedFast_PaysBiggerBonus()
        {
            _h.StartShift();
            var t = _h.Spawn(new PatronRequest(20f, 20, true));
            _h.Minutes(2);
            _h.Park(t, 2);
            _h.Minutes(20 + 3);
            _h.AtCurb(t);
            Assert.AreEqual(26, _h.Log.Last<TipEarned>().Amount);
        }

        [Test]
        public void SeveralTickets_OnlyClosedOnesScore_AndTheComboMultipliesTheSecondTip()
        {
            _h.StartShift();
            var a = _h.Spawn(new PatronRequest(10f, 20, false));
            var b = _h.Spawn(new PatronRequest(10f, 30, false));
            var c = _h.Spawn(new PatronRequest(10f, 50, false));
            _h.Minutes(1);
            _h.Park(a, 1); _h.Park(b, 2); _h.Park(c, 3);
            _h.Minutes(10);
            _h.AtCurb(a); _h.AtCurb(b);
            Assert.AreEqual(1, _h.Registry.OpenCount);
            Assert.AreEqual(22 + 32 * 2, _h.Score.Score);

            _h.Minutes(400);
            Assert.AreEqual(2, _h.Log.Count<TipEarned>());
            Assert.AreEqual(1, _h.Score.Complaints, "the third patron ran out of patience");
            Assert.AreEqual(22 + 32 * 2 - 15, _h.Log.Last<ShiftResultReady>().Result.Score);
        }

        [Test]
        public void BackToBackHappyPatrons_ClimbTheComboToTheCap_AndEarnStars()
        {
            _h.StartShift();
            for (var i = 0; i < 5; i++) Deliver();

            Assert.AreEqual(22 + 44 + 66 + 88 + 88, _h.Score.Score);
            Assert.AreEqual(4, _h.Score.ComboMultiplier);
            Assert.AreEqual(2, _h.Score.Stars);
        }

        [Test]
        public void ParkWindowRunningOut_FilesAComplaint_AndResetsTheCombo()
        {
            _h.StartShift();
            Deliver();
            Deliver();
            Assert.AreEqual(3, _h.Score.ComboMultiplier);
            Assert.AreEqual(22 + 44, _h.Score.Score);

            _h.Spawn(new PatronRequest(10f, 20, false));
            _h.Minutes(50);

            Assert.AreEqual(1, _h.Log.Count<ComplaintFiled>());
            Assert.AreEqual(1, _h.Score.Complaints);
            Assert.AreEqual(1, _h.Score.ComboMultiplier);
            Assert.AreEqual(22 + 44 - 15, _h.Score.Score);
        }

        [Test]
        public void NewShift_StartsWithACleanScore()
        {
            _h.StartShift();
            Deliver();
            _h.Minutes(1000);
            Assert.AreEqual(22, _h.Log.Last<ShiftResultReady>().Result.Score);

            _h.StartShift();
            Assert.AreEqual(0, _h.Score.Score);
            Assert.AreEqual(1, _h.Score.ComboMultiplier);
            Assert.AreEqual(0, _h.Score.Complaints);
        }

        // One happy patron: arrive, park, stay 5, leave the spot and reach the curb right away.
        private void Deliver()
        {
            var t = _h.Spawn(new PatronRequest(5f, 20, false));
            _h.Park(t, 1);
            _h.Minutes(5);
            _h.LeaveSpot(t, 1);
            _h.AtCurb(t);
        }
    }
}
