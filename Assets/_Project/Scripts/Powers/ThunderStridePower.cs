using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>
    /// Thunder stride: a temporary speed boost that lasts the level's duration, on foot and in a car (the Valet module
    /// multiplies the walking speed, the Cars module boosts the car). The cooldown starts when the duration ends. It can always start.
    /// </summary>
    public sealed class ThunderStridePower : IPower
    {
        private readonly PowerTimer _timer = new PowerTimer();
        private float _length;

        public StoryPower Power => StoryPower.ThunderStride;
        public bool IsActive => _timer.IsRunning;
        public float Remaining01 => _length > 0f ? _timer.Remaining / _length : 0f;

        public bool CanActivate() => true;

        public void Activate(PowerLevelValues values)
        {
            _length = values.Duration;
            _timer.Start(_length);
        }

        public void Tick(float deltaSeconds) => _timer.Tick(deltaSeconds);

        public void End() => _timer.Stop();
    }
}
