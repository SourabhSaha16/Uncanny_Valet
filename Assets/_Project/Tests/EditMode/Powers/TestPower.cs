using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;

namespace UncannyValet.Tests.Powers
{
    /// <summary>A trivial power for the framework tests: runs for the duration it is given (instant when 0) and records what it was started with.</summary>
    public sealed class TestPower : IPower
    {
        private readonly PowerTimer _timer = new PowerTimer();
        private float _length;

        public TestPower(StoryPower power) { Power = power; }

        public StoryPower Power { get; }
        public bool Allowed = true;
        public int Activations;
        public PowerLevelValues LastValues;

        public bool IsActive => _timer.IsRunning;
        public float Remaining01 => _length > 0f ? _timer.Remaining / _length : 0f;
        public bool CanActivate() => Allowed;

        public void Activate(PowerLevelValues values)
        {
            Activations++;
            LastValues = values;
            _length = values.Duration;
            _timer.Start(_length);
        }

        public void Tick(float deltaSeconds) => _timer.Tick(deltaSeconds);
        public void End() => _timer.Stop();
    }

    /// <summary>A bucket supply for the framework tests.</summary>
    public sealed class FakeBucket : IPowerBucket
    {
        public int Open;
        public int Spent;
        public bool CanSpend => Open > 0;

        public bool TrySpend()
        {
            if (Open <= 0) return false;
            Open--;
            Spent++;
            return true;
        }
    }
}
