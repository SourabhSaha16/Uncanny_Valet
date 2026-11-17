using System;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>The authored numbers of one power level (base or upgraded), as stored in a <see cref="PowerDefinition"/>.</summary>
    [Serializable]
    public sealed class PowerLevelData
    {
        [SerializeField, Min(0f), Tooltip("Seconds before the power can be used again, counted after its use ends.")]
        private float _cooldown;
        [SerializeField, Min(0f), Tooltip("Seconds the power lasts; 0 for an instant power.")]
        private float _duration;
        [SerializeField, Min(0f), Tooltip("Distance or range in world units (dash distance, gust range, recall range).")]
        private float _distance;
        [SerializeField, Min(0f), Tooltip("How hard the power pushes things.")]
        private float _pushStrength;
        [SerializeField, Min(0f), Tooltip("Width of the power's reach in world units.")]
        private float _width;
        [SerializeField, Min(0f), Tooltip("A second time or number a power needs (Thunder stride: the on-foot dash time in seconds).")]
        private float _secondary;

        public PowerLevelData() { }

        public PowerLevelData(PowerLevelValues values)
        {
            _cooldown = values.Cooldown;
            _duration = values.Duration;
            _distance = values.Distance;
            _pushStrength = values.PushStrength;
            _width = values.Width;
            _secondary = values.Secondary;
        }

        /// <summary>The numbers as the plain struct the rest of the game passes around.</summary>
        public PowerLevelValues ToValues() => new PowerLevelValues(_cooldown, _duration, _distance, _pushStrength, _width, _secondary);
    }
}
