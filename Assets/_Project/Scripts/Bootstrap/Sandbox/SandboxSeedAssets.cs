using System;
using UncannyValet.Audio;
using UncannyValet.Core.Config;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Patrons;
using UncannyValet.Powers;
using UncannyValet.Traffic;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The shared config assets whose numbers fill the tuning store, so every row of the F2 panel has a value. Nothing here runs in the sandbox; the panel only needs the keys.</summary>
    [Serializable]
    public sealed class SandboxSeedAssets
    {
        [SerializeField] private ShiftConfig _shift;
        [SerializeField] private PatronConfig _patrons;
        [SerializeField] private BlessingConfig _blessings;
        [SerializeField] private DirectorConfig _director;
        [SerializeField] private DirectorProfile _profile;
        [SerializeField] private TrafficConfig _traffic;
        [SerializeField] private PowerBarConfig _powers;
        [SerializeField] private AudioConfig _audio;
        [SerializeField] private HudEmotionConfig _emotions;

        public ShiftConfig Shift => _shift;
        public PatronConfig Patrons => _patrons;
        public BlessingConfig Blessings => _blessings;
        public DirectorConfig Director => _director;
        public DirectorProfile Profile => _profile;
        public TrafficConfig Traffic => _traffic;
        public PowerBarConfig Powers => _powers;
        public AudioConfig Audio => _audio;
        public HudEmotionConfig Emotions => _emotions;

        /// <summary>True when the assets the seeding cannot do without are set.</summary>
        public bool IsComplete => _shift != null && _patrons != null && _blessings != null;
    }
}
