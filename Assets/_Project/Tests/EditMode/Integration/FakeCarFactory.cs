using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>Unity-free stand-in for the Cars module. Mimics CarDriver: events only on a real change.</summary>
    public sealed class FakeCarFactory : ICarFactory
    {
        public sealed class FakeCar : IDrivable
        {
            private readonly IEventBus _bus;
            public FakeCar(CarId id, IEventBus bus) { Id = id; _bus = bus; }
            public CarId Id { get; }
            public Transform Transform => null;
            public float Speed { get; set; }
            public bool HasDriver { get; private set; }
            public void SetInput(float throttle, float steer) { }

            public void SetDriver(bool hasDriver)
            {
                if (HasDriver == hasDriver) return;
                HasDriver = hasDriver;
                if (hasDriver) _bus.Publish(new CarEntered(Id));
                else _bus.Publish(new CarExited(Id));
            }
        }

        private readonly IEventBus _bus;
        private readonly Dictionary<CarId, FakeCar> _cars = new Dictionary<CarId, FakeCar>();

        public FakeCarFactory(IEventBus bus) { _bus = bus; }

        public int DespawnCount { get; private set; }
        public int Alive => _cars.Count;

        public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees)
        {
            var car = new FakeCar(id, _bus);
            _cars[id] = car;
            return car;
        }

        public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees)
            => Spawn(id, stopPosition, rotationDegrees);

        public void DriveAwayAndDespawn(CarId id) => Despawn(id);

        public bool TryGet(CarId id, out IDrivable car)
        {
            var found = _cars.TryGetValue(id, out var fake);
            car = fake;
            return found;
        }

        public void Despawn(CarId id)
        {
            if (_cars.Remove(id)) DespawnCount++;
        }
    }
}
