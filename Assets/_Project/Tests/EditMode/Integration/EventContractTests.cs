using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Patrons;
using UncannyValet.Valet;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>Events are published exactly once, in the agreed order, and never go negative.</summary>
    public class EventContractTests
    {
        private ShiftHarness _h;

        [SetUp] public void SetUp() { _h = new ShiftHarness(); _h.StartShift(); }
        [TearDown] public void TearDown() { _h.Dispose(); }

        private Ticket ParkedTicket(float stay = 10f, int baseBlessing = 20)
        {
            var t = _h.Spawn(new PatronRequest(stay, baseBlessing, false));
            _h.Minutes(1);
            _h.Park(t, 1);
            return t;
        }

        [Test]
        public void Spawn_OpensTicketBeforeCarSpawned_AndCarExistsAlready()
        {
            var carExistedOnOpen = false;
            _h.Bus.Subscribe<TicketOpened>(e => carExistedOnOpen = _h.Cars.TryGet(e.Car, out _));
            var t = _h.Spawn(new PatronRequest(10f, 20, false));
            Assert.IsTrue(carExistedOnOpen);
            Assert.AreEqual(1, _h.Log.Count<TicketOpened>());
            Assert.AreEqual(1, _h.Log.Count<CarSpawned>());
            Assert.Less(_h.Log.IndexOf<TicketOpened>(), _h.Log.IndexOf<CarSpawned>());
            Assert.AreEqual(t.TicketId, _h.Log.Last<CarSpawned>().Ticket);
        }

        [Test]
        public void CarAtCurb_PublishedTwice_ClosesOnce()
        {
            var t = ParkedTicket();
            _h.Minutes(10);
            _h.AtCurb(t);
            _h.AtCurb(t);
            Assert.AreEqual(1, _h.Log.Count<TicketClosed>());
            Assert.AreEqual(1, _h.Log.Count<BlessingEarned>());
            Assert.AreEqual(1, _h.Log.Count<CarDeparted>());
            Assert.AreEqual(1, _h.Cars.DespawnCount);
            Assert.AreEqual(22, _h.Blessings.Blessings);
        }

        [Test]
        public void PickupRequested_OncePerTicket_EvenAfterManyTicks()
        {
            ParkedTicket();
            _h.Minutes(100);
            Assert.AreEqual(1, _h.Log.Count<PickupRequested>());
        }

        [Test]
        public void CarParked_Repeated_DoesNotRestartTheStay()
        {
            var t = ParkedTicket(10f);
            _h.Minutes(5);
            _h.Bus.Publish(new CarParked(t.CarId, 1));
            _h.Minutes(5);
            Assert.AreEqual(TicketState.PickupRequested, t.State);
            Assert.AreEqual(1, _h.Log.Count<PickupRequested>());
        }

        [Test]
        public void ReparkWhileWaitingForPickup_KeepsTheTicketRequested()
        {
            var t = ParkedTicket(10f);
            _h.Minutes(10);
            _h.LeaveSpot(t, 1);
            _h.Park(t, 2);
            Assert.AreEqual(TicketState.PickupRequested, t.State);
        }

        [Test]
        public void ContestedSpot_OnlyFirstCarIsParked()
        {
            var a = _h.Spawn(new PatronRequest(10f, 20, false));
            var b = _h.Spawn(new PatronRequest(10f, 20, false));
            Assert.IsTrue(_h.Park(a, 1));
            Assert.IsFalse(_h.Park(b, 1));
            Assert.AreEqual(1, _h.Log.Count<CarParked>());
            Assert.AreEqual(TicketState.AwaitingPark, b.State);
            Assert.IsTrue(_h.LeaveSpot(a, 1));
            Assert.IsFalse(_h.LeaveSpot(a, 1), "leaving twice must not publish twice");
            Assert.AreEqual(1, _h.Log.Count<CarLeftSpot>());
        }

        [Test]
        public void CarAtCurb_BeforePickup_PaysNothing()
        {
            var t = ParkedTicket(30f);
            _h.AtCurb(t);
            Assert.AreEqual(0, _h.Log.Count<BlessingEarned>());
            Assert.AreEqual(1, _h.Registry.OpenCount);
        }

        [Test]
        public void ShiftEnds_WithOpenTicket_ClosesItWithNoBlessingAndEndsOnce()
        {
            ParkedTicket(30f);
            _h.Minutes(500);
            Assert.AreEqual(1, _h.Log.Count<ShiftEnded>());
            // Every open ticket closes with $0 when the shift ends.
            Assert.AreEqual(0, _h.Registry.OpenCount);
        }

        [Test]
        public void SeatAndCar_EnterAndExitPublishOnce()
        {
            var t = _h.Spawn(new PatronRequest(10f, 20, false));
            _h.Cars.TryGet(t.CarId, out var car);
            var state = new ValetState();
            // The same reaction SeatController has: seat the valet, then SetDriver(true) again (a no-op).
            _h.Bus.Subscribe<CarEntered>(e => { if (state.TryEnterCar()) car.SetDriver(true); });

            car.SetDriver(true);
            Assert.IsTrue(state.IsDriving);
            Assert.AreEqual(1, _h.Log.Count<CarEntered>());

            car.SetDriver(false);
            car.SetDriver(false);
            Assert.AreEqual(1, _h.Log.Count<CarExited>());
            Assert.IsTrue(state.TryExitCar());
        }

        [Test]
        public void NearestPicker_SkipsCarsWithADriver()
        {
            var positions = new[] { new Vector2(1f, 0f), new Vector2(2f, 0f), new Vector2(9f, 0f) };
            var occupied = new[] { true, false, false };
            var pick = NearestPicker.Pick(Vector2.zero, positions, 3f, i => !occupied[i]);
            Assert.AreEqual(1, pick);
            Assert.AreEqual(-1, NearestPicker.Pick(Vector2.zero, positions, 0.5f));
        }
    }
}
