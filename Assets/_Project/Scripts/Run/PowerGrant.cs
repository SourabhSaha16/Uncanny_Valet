using System;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>The power or upgrade a lot grants when cleared: a power and the level it reaches (1 earned, 2 upgraded).</summary>
    [Serializable]
    public struct PowerGrant
    {
        public const int MaxLevel = 2;

        [SerializeField, Tooltip("None for lots that grant nothing (every training lot).")]
        private StoryPower _power;
        [SerializeField, Range(1, MaxLevel), Tooltip("1 earns the power, 2 upgrades it.")]
        private int _level;

        public PowerGrant(StoryPower power, int level)
        {
            _power = power;
            _level = level;
        }

        public StoryPower Power => _power;
        public int Level => _level;
        public bool IsNone => _power == StoryPower.None;

        /// <summary>True for no grant, or a grant at level 1 or 2.</summary>
        public bool IsValid => IsNone || (_level >= 1 && _level <= MaxLevel);
    }
}
