using NUnit.Framework;
using UncannyValet.Core.Events;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Integration
{
    public class FullNightTests
    {
        private NightHarness _h;

        [SetUp] public void SetUp() { _h = new NightHarness(1000); }
        [TearDown] public void TearDown() { _h.Dispose(); }

        // Tip config defaults: window 10 min, bonus +2, penalty 0.5 per minute late.
        [Test]
        public void WholeNight_TicketToTipToDebt()
        {
            _h.StartNight(1);
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
            Assert.AreEqual(22, _h.Ledger.TipsTonight);
            Assert.AreEqual(1000, _h.Ledger.DebtRemaining, "tips only reach the debt at night end");
            Assert.AreEqual(0, _h.Registry.OpenCount);
            Assert.AreEqual(1, _h.Cars.DespawnCount);

            _h.Minutes(400);
            Assert.AreEqual(1, _h.Log.Count<NightEnded>());
            Assert.AreEqual(978, _h.Ledger.DebtRemaining);
            Assert.AreEqual(0, _h.Ledger.TipsTonight);
            Assert.AreEqual(978, _h.Log.Last<DebtChanged>().DebtRemaining);
        }

        [Test]
        public void EventOrder_FollowsTheLoop()
        {
            _h.StartNight(1);
            var t = _h.Spawn(new PatronRequest(10f, 20, false));
            _h.Minutes(1);
            _h.Park(t, 1);
            _h.Minutes(10);
            _h.AtCurb(t);
            var l = _h.Log;
            Assert.Less(l.IndexOf<NightStarted>(), l.IndexOf<TicketOpened>());
            Assert.Less(l.IndexOf<TicketOpened>(), l.IndexOf<CarParked>());
            Assert.Less(l.IndexOf<CarParked>(), l.IndexOf<PickupRequested>());
            Assert.Less(l.IndexOf<PickupRequested>(), l.IndexOf<TicketClosed>());
            Assert.Less(l.IndexOf<TicketClosed>(), l.IndexOf<TipEarned>());
            Assert.Less(l.IndexOf<TipEarned>(), l.IndexOf<DebtChanged>());
            Assert.Less(l.IndexOf<TicketClosed>(), l.IndexOf<CarDeparted>());
        }

        [Test]
        public void VeryLateReturn_TipIsZeroNotNegative()
        {
            _h.StartNight(1);
            var t = _h.Spawn(new PatronRequest(30f, 20, false));
            _h.Minutes(5);
            _h.Park(t, 1);
            _h.Minutes(30 + 100);
            _h.AtCurb(t);
            Assert.AreEqual(0, _h.Log.Last<TipEarned>().Amount);
            Assert.AreEqual(0, _h.Ledger.TipsTonight);
        }

        [Test]
        public void ImpatientPatron_ReturnedFast_PaysBiggerBonus()
        {
            _h.StartNight(1);
            var t = _h.Spawn(new PatronRequest(20f, 20, true));
            _h.Minutes(2);
            _h.Park(t, 2);
            _h.Minutes(20 + 3);
            _h.AtCurb(t);
            Assert.AreEqual(26, _h.Log.Last<TipEarned>().Amount);
        }

        [Test]
        public void SeveralTickets_OnlyClosedOnesPayTheDebt()
        {
            _h.StartNight(1);
            var a = _h.Spawn(new PatronRequest(10f, 20, false));
            var b = _h.Spawn(new PatronRequest(10f, 30, false));
            var c = _h.Spawn(new PatronRequest(10f, 50, false));
            _h.Minutes(1);
            _h.Park(a, 1); _h.Park(b, 2); _h.Park(c, 3);
            _h.Minutes(10);
            _h.AtCurb(a); _h.AtCurb(b);
            Assert.AreEqual(1, _h.Registry.OpenCount);

            _h.Minutes(400);
            Assert.AreEqual(1000 - 22 - 32, _h.Ledger.DebtRemaining);
            Assert.AreEqual(2, _h.Log.Count<TipEarned>());
        }

        [Test]
        public void TipsLargerThanDebt_LeaveZeroDebt()
        {
            _h.Dispose();
            _h = new NightHarness(10);
            _h.StartNight(1);
            var t = _h.Spawn(new PatronRequest(5f, 20, false));
            _h.Minutes(1);
            _h.Park(t, 1);
            _h.Minutes(5);
            _h.AtCurb(t);
            _h.Minutes(400);
            Assert.AreEqual(0, _h.Ledger.DebtRemaining);
        }
    }
}
