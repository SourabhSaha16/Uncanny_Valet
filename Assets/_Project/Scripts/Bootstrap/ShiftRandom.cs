using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// The one random source of a shift, shared by the patron spawner and the director. It is reseeded when a shift
    /// starts (also on Retry): the DirSeed tuning value plus the shift number, or a fresh seed when DirSeed is 0.
    /// </summary>
    public sealed class ShiftRandom : IPatronRandom, IDirectorRandom, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly ITuning _tuning;
        private SeededPatronRandom _inner;
        private int _shiftNumber;

        public ShiftRandom(IEventBus bus, ITuning tuning)
        {
            _bus = bus; _tuning = tuning;
            _inner = new SeededPatronRandom(Environment.TickCount);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        public float Value() => _inner.Value();

        public float Range(float min, float max) => _inner.Range(min, max);

        public void Dispose() => _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);

        private void OnShiftStarted(ShiftStarted e)
        {
            _shiftNumber++;
            var fixedSeed = _tuning.TryGet(TuningKey.DirSeed, out var v) ? (int)v : 0;
            var seed = fixedSeed != 0 ? fixedSeed + _shiftNumber : Environment.TickCount + _shiftNumber;
            _inner = new SeededPatronRandom(seed);
            Debug.Log("Shift " + _shiftNumber + " random seed " + seed);
        }
    }
}
