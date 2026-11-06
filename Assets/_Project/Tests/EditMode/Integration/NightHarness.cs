using System;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Night;
using UncannyValet.Patrons;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UncannyValet.Tests.Integration
{
    /// <summary>
    /// Wires the real EventBus, NightClock, TicketRegistry, TipCalculator, DebtLedger, PatronDeparture and
    /// SpotOccupancy. The scene-bound pieces (spawner, parking zones) are replaced by the small helper
    /// methods below, which publish the same events in the same order as the real code.
    /// </summary>
    public sealed class NightHarness : IDisposable
    {
        private readonly GameObject _clockObject = new GameObject("test-clock");
        private readonly PatronDeparture _departure;
        private int _nextCar = 1;

        public readonly EventBus Bus = new EventBus();
        public readonly NightClock Clock;
        public readonly FakeCarFactory Cars;
        public readonly TicketRegistry Registry;
        public readonly DebtLedger Ledger;
        public readonly SpotOccupancy Spots = new SpotOccupancy(new[] { 1, 2, 3, 4 });
        public readonly EventLog Log;

        public NightHarness(int startingDebt = 1000)
        {
            Log = new EventLog(Bus);
            Clock = _clockObject.AddComponent<NightClock>();
            Clock.Initialize(Bus, 0, 1f); // pin 1 game minute per second so Minutes(n) is exact
            Cars = new FakeCarFactory(Bus);
            var tips = new TipCalculator(ScriptableObject.CreateInstance<TipConfig>());
            Registry = new TicketRegistry(Bus, Clock, tips);
            Ledger = new DebtLedger(Bus, startingDebt);
            _departure = new PatronDeparture(Bus, Cars);
        }

        public void StartNight(int night = 1) => Clock.StartNight(night);

        /// <summary>Runs whole game minutes (one real second each: the harness pins the clock at 1 game minute per second).</summary>
        public void Minutes(int minutes)
        {
            for (var i = 0; i < minutes; i++) Clock.Advance(1f);
        }

        /// <summary>Same order as PatronSpawner.Spawn: spawn car, open ticket, publish CarSpawned.</summary>
        public Ticket Spawn(PatronRequest request)
        {
            var car = new CarId(_nextCar++);
            Cars.Spawn(car, Vector2.zero, 0f);
            var ticket = Registry.Open(car, request);
            Bus.Publish(new CarSpawned(car, ticket.TicketId));
            return ticket;
        }

        /// <summary>Like ParkingSpot.OnSettled: publishes only when the occupancy accepts the car.</summary>
        public bool Park(Ticket t, int spot)
        {
            if (!Spots.Park(spot, t.CarId)) return false;
            Bus.Publish(new CarParked(t.CarId, spot));
            return true;
        }

        public bool LeaveSpot(Ticket t, int spot)
        {
            if (!Spots.Leave(spot, t.CarId)) return false;
            Bus.Publish(new CarLeftSpot(t.CarId, spot));
            return true;
        }

        public void AtCurb(Ticket t) => Bus.Publish(new CarAtCurb(t.CarId));

        public void Dispose()
        {
            _departure.Dispose();
            Registry.Dispose();
            Ledger.Dispose();
            Object.DestroyImmediate(_clockObject);
        }
    }
}
