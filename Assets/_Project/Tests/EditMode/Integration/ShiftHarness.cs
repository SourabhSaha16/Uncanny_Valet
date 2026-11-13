using System;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Night;
using UncannyValet.Patrons;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UncannyValet.Tests.Integration
{
    /// <summary>
    /// Wires the real EventBus, ShiftClock, TicketRegistry, BlessingCalculator, ShiftBlessings, PatronDeparture and
    /// SpotOccupancy. The scene-bound pieces (spawner, parking zones) are replaced by the small helper
    /// methods below, which publish the same events in the same order as the real code. The blessing config has no
    /// patience bonus, so blessings stay easy to add up by hand (the favour still applies).
    /// </summary>
    public sealed class ShiftHarness : IDisposable
    {
        private readonly GameObject _clockObject = new GameObject("test-clock");
        private readonly PatronDeparture _departure;
        private readonly ShiftConfig _config;
        private readonly BlessingConfig _blessingConfig;
        private int _nextCar = 1;

        public readonly EventBus Bus = new EventBus();
        public readonly ShiftClock Clock;
        public readonly FakeCarFactory Cars;
        public readonly TicketRegistry Registry;
        public readonly ShiftBlessings Blessings;

        /// <summary>The lot goal the shift is judged against at its end; null judges nothing.</summary>
        public ILotGoal Goal;
        public readonly SpotOccupancy Spots = new SpotOccupancy(new[] { 1, 2, 3, 4 });
        public readonly EventLog Log;

        public ShiftHarness()
        {
            Log = new EventLog(Bus);
            _config = ScriptableObject.CreateInstance<ShiftConfig>();
            Clock = _clockObject.AddComponent<ShiftClock>();
            Clock.Initialize(Bus, _config); // the config default is 1 game minute per second, so Minutes(n) is exact
            Cars = new FakeCarFactory(Bus);
            _blessingConfig = ScriptableObject.CreateInstance<BlessingConfig>();
            var so = new SerializedObject(_blessingConfig);
            so.FindProperty("_maxPatienceBonus").intValue = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            Blessings = new ShiftBlessings(Bus, _config, () => Goal);
            Registry = new TicketRegistry(Bus, Clock, new BlessingCalculator(_blessingConfig, null, ModifierTestKit.Catalog()), Blessings);
            _departure = new PatronDeparture(Bus, Cars);
        }

        public void StartShift() => Clock.StartShift();

        /// <summary>Runs whole game minutes (one real second each: the config default is 1 game minute per second).</summary>
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
            Bus.Publish(new CarClearedQueue(car));
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
            Blessings.Dispose();
            Object.DestroyImmediate(_clockObject);
            Object.DestroyImmediate(_config);
            Object.DestroyImmediate(_blessingConfig);
        }
    }
}
