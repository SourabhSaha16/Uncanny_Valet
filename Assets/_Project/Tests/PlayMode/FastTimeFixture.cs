using NUnit.Framework;
using UnityEngine;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Runs every PlayMode test with simulated time at <see cref="Speed"/> times real time. The physics step stays
    /// the fixed 0.02 s, so the simulation is step for step the same as at 1x; only fewer real seconds are spent waiting.
    /// </summary>
    [SetUpFixture]
    public sealed class FastTimeFixture
    {
        private const float Speed = 4f;
        private float _previous;

        [OneTimeSetUp]
        public void SpeedUp()
        {
            _previous = Time.timeScale;
            Time.timeScale = Speed;
        }

        [OneTimeTearDown]
        public void Restore() => Time.timeScale = _previous;
    }
}
