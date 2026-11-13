using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Powers
{
    /// <summary>World time is scaled down (0 = frozen) for the level's duration. Provides <see cref="IWorldTime"/>.</summary>
    public sealed class FreezeTimeEffect : IPower, IWorldTime
    {
        private readonly PowerSettings _settings;
        private readonly PowerTimer _timer = new PowerTimer();
        private float _length;

        public FreezeTimeEffect(PowerSettings settings) { _settings = settings; }

        public StoryPower Power => StoryPower.FreezeTime;
        public bool IsActive => _timer.IsRunning;

        /// <summary>Seconds of freeze left.</summary>
        public float Remaining => _timer.Remaining;

        public float Remaining01 => _length > 0f ? _timer.Remaining / _length : 0f;
        public float Scale => IsActive ? _settings.FreezeTimeScale : 1f;
        public bool IsSlowed => Scale < 1f;

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
