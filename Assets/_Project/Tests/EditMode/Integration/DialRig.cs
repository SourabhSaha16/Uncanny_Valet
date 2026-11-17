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
        /// <param name="anchorDefaults">True moves every dial to the anchor-default position (5) after seeding, so every key
        /// holds its authored value; false leaves the dials at the seeded baseline.</param>
        public DialRig(bool linked = true, bool anchorDefaults = true)
        {
            Tuning = new TuningStore(Bus);
            var shift = ScriptableObject.CreateInstance<ShiftConfig>();
            var patrons = ScriptableObject.CreateInstance<PatronConfig>();
            var blessings = ScriptableObject.CreateInstance<BlessingConfig>();
            var director = ScriptableObject.CreateInstance<DirectorConfig>();
            Profile = ScriptableObject.CreateInstance<DirectorProfile>();
            _assets = new ScriptableObject[] { shift, patrons, blessings, director, Profile };
            Link = TuningSeeder.Seed(Tuning, shift, patrons, blessings, 6, director, Profile, bus: linked ? Bus : null);
            if (linked && anchorDefaults)
                foreach (var key in DifficultyDials.DialKeys) Tuning.Set(key, AnchorDefault);
            Bus.Subscribe<TuningChanged>(OnChanged);
        }

        /// <summary>The dial position at which every anchor is neutral.</summary>
        public const float AnchorDefault = 5f;

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
