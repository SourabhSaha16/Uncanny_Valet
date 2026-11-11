using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;

namespace UncannyValet.Tests.Core
{
    /// <summary>How modules scale their own delta time by the freeze time contract.</summary>
    public class WorldTimeReaderTests
    {
        private sealed class StubTime : IWorldTime
        {
            public float Value = 1f;
            public float Scale => Value;
            public bool IsSlowed => Value < 1f;
        }

        [SetUp]
        public void SetUp() => Services.Reset();

        [TearDown]
        public void TearDown() => Services.Reset();

        [Test]
        public void NoContract_PassesTimeThrough()
        {
            var reader = new WorldTimeReader();
            Assert.AreEqual(0.5f, reader.Scaled(0.5f), 0.0001f);
            Assert.AreEqual(1f, reader.Scale);
            Assert.IsFalse(reader.IsSlowed);
        }

        [Test]
        public void Frozen_ScalesTimeToZero()
        {
            Services.Register<IWorldTime>(new StubTime { Value = 0f });
            var reader = new WorldTimeReader();
            Assert.AreEqual(0f, reader.Scaled(0.5f));
            Assert.IsTrue(reader.IsSlowed);
        }

        [Test]
        public void Slowed_ScalesTimeProportionally()
        {
            Services.Register<IWorldTime>(new StubTime { Value = 0.3f });
            var reader = new WorldTimeReader();
            Assert.AreEqual(0.3f, reader.Scaled(1f), 0.0001f);
        }

        [Test]
        public void NegativeScale_NeverRunsTimeBackwards()
        {
            Services.Register<IWorldTime>(new StubTime { Value = -2f });
            var reader = new WorldTimeReader();
            Assert.AreEqual(0f, reader.Scaled(0.25f));
            Assert.AreEqual(0f, reader.Scale);
        }

        [Test]
        public void ContractRegisteredLate_IsPickedUpWithinTheRefreshTime()
        {
            var reader = new WorldTimeReader();
            Assert.AreEqual(0.1f, reader.Scaled(0.1f), 0.0001f);
            Services.Register<IWorldTime>(new StubTime { Value = 0f });
            Assert.AreEqual(0.1f, reader.Scaled(0.1f), 0.0001f, "not looked up again yet");
            Assert.AreEqual(0f, reader.Scaled(WorldTimeReader.RefreshSeconds), "looked up after the refresh time");
            Assert.AreEqual(0f, reader.Scaled(0.1f));
        }

        [Test]
        public void ScaleFollowsTheContractLive_WithoutAnotherLookup()
        {
            var time = new StubTime { Value = 1f };
            Services.Register<IWorldTime>(time);
            var reader = new WorldTimeReader();
            Assert.AreEqual(0.2f, reader.Scaled(0.2f), 0.0001f);
            time.Value = 0f;
            Assert.AreEqual(0f, reader.Scaled(0.2f));
            time.Value = 1f;
            Assert.AreEqual(0.2f, reader.Scaled(0.2f), 0.0001f);
        }
    }
}
