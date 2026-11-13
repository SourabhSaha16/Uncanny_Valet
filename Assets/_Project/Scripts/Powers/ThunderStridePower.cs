using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>
    /// Thunder stride: one power, two uses. On foot it lasts the dash time (the Valet module does the dash); in a car it lasts
    /// the boost time (the Cars module does the boost). The cooldown starts when that time ends. It can always start.
    /// </summary>
    public sealed class ThunderStridePower : IPower
    {
        private readonly Func<bool> _isDriving;
        private readonly PowerTimer _timer = new PowerTimer();
        private float _length;

        /// <param name="isDriving">True while the valet sits in a car.</param>
        public ThunderStridePower(Func<bool> isDriving) { _isDriving = isDriving ?? throw new ArgumentNullException(nameof(isDriving)); }

        public StoryPower Power => StoryPower.ThunderStride;
        public bool IsActive => _timer.IsRunning;
        public float Remaining01 => _length > 0f ? _timer.Remaining / _length : 0f;

        public bool CanActivate() => true;

        /// <summary>How long a use lasts at a level's numbers: the boost time in a car, the dash time on foot.</summary>
        public static float UseSeconds(PowerLevelValues values, bool driving) => driving ? values.Duration : values.Secondary;

        public void Activate(PowerLevelValues values)
        {
            _length = UseSeconds(values, _isDriving());
            _timer.Start(_length);
        }

        public void Tick(float deltaSeconds) => _timer.Tick(deltaSeconds);

        public void End() => _timer.Stop();
    }
}
