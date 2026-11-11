using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>Test stand-in for the car factory whose cars carry a real transform (position and heading).</summary>
    public sealed class TransformCarFactory : ICarFactory
    {
        private sealed class Car : IDrivable
        {
            public Car(CarId id, Transform transform) { Id = id; Transform = transform; }
            public CarId Id { get; }
            public Transform Transform { get; }
            public float Speed => 0f;
            public bool HasDriver => false;
            public void SetInput(float throttle, float steer) { }
            public void SetDrift(bool drift) { }
            public void SetDriver(bool hasDriver) { }
        }

        private readonly Dictionary<CarId, IDrivable> _cars = new Dictionary<CarId, IDrivable>();

        public void Add(CarId id, Transform transform) => _cars[id] = new Car(id, transform);

        public IDrivable Spawn(CarId id, Vector2 position, float rotationDegrees) => null;
        public IDrivable SpawnArriving(CarId id, Vector2 stopPosition, float rotationDegrees, int arrivalSlot) => null;
        public void DriveAwayAndDespawn(CarId id) { }
        public bool TryGet(CarId id, out IDrivable car) => _cars.TryGetValue(id, out car);
        public void Despawn(CarId id) { }
    }
}
