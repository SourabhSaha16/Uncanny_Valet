using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Audio
{
    /// <summary>The live F2 volumes of the themed lot sounds: ambience, last-30-seconds layer, and the chime, splash and squelch.</summary>
    public sealed class WorldSoundTuning
    {
        private readonly IEventBus _bus;
        private readonly Action _changed;
        private bool _active;

        public WorldSoundTuning(IEventBus bus, float ambience, float escalation, float worldSound, Action changed)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _changed = changed;
            Ambience = ambience;
            Escalation = escalation;
            WorldSound = worldSound;
        }

        public float Ambience { get; private set; }
        public float Escalation { get; private set; }
        public float WorldSound { get; private set; }

        /// <summary>Takes the values already in the store.</summary>
        public void Load(ITuning store)
        {
            if (store.TryGet(TuningKey.AmbienceVolume, out var ambience)) Ambience = ambience;
            if (store.TryGet(TuningKey.EscalationVolume, out var escalation)) Escalation = escalation;
            if (store.TryGet(TuningKey.WorldSoundVolume, out var world)) WorldSound = world;
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
            if (e.Key == TuningKey.AmbienceVolume) Ambience = e.Value;
            else if (e.Key == TuningKey.EscalationVolume) Escalation = e.Value;
            else if (e.Key == TuningKey.WorldSoundVolume) WorldSound = e.Value;
            else return;
            _changed?.Invoke();
        }
    }
}
