using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Audio
{
    /// <summary>The live 'Tyre sound volume' and 'Tyre sound sharpness' numbers: read from the tuning store at start, then follow F2 changes.</summary>
    public sealed class TyreSoundTuning
    {
        private readonly IEventBus _bus;
        private readonly Action _changed;
        private bool _active;

        /// <param name="volume">Volume used until the store says otherwise (0..1).</param>
        /// <param name="sharpness">Sharpness used until the store says otherwise (0..1).</param>
        /// <param name="changed">Called after either value changes from the panel.</param>
        public TyreSoundTuning(IEventBus bus, float volume, float sharpness, Action changed)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _changed = changed;
            Volume = volume;
            Sharpness = sharpness;
        }

        /// <summary>Volume multiplier of the tyre sounds, 0..1.</summary>
        public float Volume { get; private set; }

        /// <summary>Brightness of the tyre sounds, 0..1.</summary>
        public float Sharpness { get; private set; }

        /// <summary>Takes the values already in the store (the panel may have changed them before the audio started).</summary>
        public void Load(ITuning store)
        {
            if (store.TryGet(TuningKey.TyreSoundVolume, out var volume)) Volume = volume;
            if (store.TryGet(TuningKey.TyreSoundSharpness, out var sharpness)) Sharpness = sharpness;
        }

        public void Subscribe()
        {
            if (_active) return;
            _active = true;
            _bus.Subscribe<TuningChanged>(OnChanged);
        }

        public void Unsubscribe()
        {
            if (!_active) return;
            _active = false;
            _bus.Unsubscribe<TuningChanged>(OnChanged);
        }

        private void OnChanged(TuningChanged e)
        {
            if (e.Key == TuningKey.TyreSoundVolume) Volume = e.Value;
            else if (e.Key == TuningKey.TyreSoundSharpness) Sharpness = e.Value;
            else return;
            _changed?.Invoke();
        }
    }
}
