using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.UI
{
    /// <summary>Tracks which cars rest in the curb zone facing the wrong way, from CurbFacingChanged events only.</summary>
    public sealed class CurbHintTracker : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly List<CarId> _wrong = new List<CarId>(4);

        public CurbHintTracker(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<CurbFacingChanged>(OnChanged);
        }

        /// <summary>The cars that currently need a hint.</summary>
        public IReadOnlyList<CarId> WrongCars => _wrong;

        public void Dispose() => _bus.Unsubscribe<CurbFacingChanged>(OnChanged);

        private void OnChanged(CurbFacingChanged e)
        {
            var index = _wrong.IndexOf(e.Car);
            if (e.Wrong && index < 0) _wrong.Add(e.Car);
            else if (!e.Wrong && index >= 0) _wrong.RemoveAt(index);
        }
    }
}
