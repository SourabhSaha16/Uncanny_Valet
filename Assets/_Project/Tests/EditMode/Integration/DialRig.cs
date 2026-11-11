using System;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>A tuning store seeded from default configs with the difficulty dials linked, for the dial tests.</summary>
    public sealed class DialRig : IDisposable
    {
        private readonly ScriptableObject[] _assets;
        public readonly EventBus Bus = new EventBus();
        public readonly TuningStore Tuning;
        public readonly DirectorProfile Profile;
        public readonly DifficultyDialsLink Link;
        public int Changes;

        /// <param name="linked">False seeds only (no dial link), giving the authored values to compare against.</param>
        public DialRig(bool linked = true)
        {
            Tuning = new TuningStore(Bus);
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var tips = ScriptableObject.CreateInstance<TipConfig>();
            var director = ScriptableObject.CreateInstance<DirectorConfig>();
            Profile = ScriptableObject.CreateInstance<DirectorProfile>();
            _assets = new ScriptableObject[] { shift, patrons, tips, director, Profile };
            Link = TuningSeeder.Seed(Tuning, shift, patrons, tips, 6, director, Profile, bus: linked ? Bus : null);
            Bus.Subscribe<TuningChanged>(OnChanged);
        }

        public float this[TuningKey key] => Tuning.Get(key);

        public void Dial(TuningKey dial, float value) => Tuning.Set(dial, value);

        public void Dispose()
        {
            Bus.Unsubscribe<TuningChanged>(OnChanged);
            Link?.Dispose();
            foreach (var asset in _assets) UnityEngine.Object.DestroyImmediate(asset);
        }

        private void OnChanged(TuningChanged e) => Changes++;
    }
}
