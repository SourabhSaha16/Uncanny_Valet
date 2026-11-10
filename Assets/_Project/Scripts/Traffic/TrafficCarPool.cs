using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>Pool of <see cref="TrafficCar"/> objects. All are created up front so nothing is instantiated while driving.</summary>
    public sealed class TrafficCarPool
    {
        private readonly Stack<TrafficCar> _free;

        public TrafficCarPool(Transform parent, int size)
        {
            _free = new Stack<TrafficCar>(size);
            for (int i = 0; i < size; i++) _free.Push(Create(parent, i));
        }

        /// <summary>Takes a hidden car, or null when the pool is empty.</summary>
        public TrafficCar Rent() => _free.Count > 0 ? _free.Pop() : null;

        /// <summary>Hides a car and gives it back.</summary>
        public void Return(TrafficCar car)
        {
            car.Hide();
            _free.Push(car);
        }

        private static TrafficCar Create(Transform parent, int index)
        {
            var go = new GameObject("TrafficCar" + index);
            go.transform.SetParent(parent, false);
            var car = go.AddComponent<TrafficCar>();
            go.SetActive(false);
            return car;
        }
    }
}
