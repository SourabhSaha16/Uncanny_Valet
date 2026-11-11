using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class GatedInputSourceTests
    {
        private sealed class Fake : IInputSource
        {
            public Vector2 Move => Vector2.right;
            public bool InteractPressed => true;
            public bool Drift => true;
        }

        [Test]
        public void PassesInputThroughWhenNotBlocked()
        {
            var gate = new GatedInputSource(new Fake(), () => false);
            Assert.AreEqual(Vector2.right, gate.Move);
            Assert.IsTrue(gate.InteractPressed);
            Assert.IsTrue(gate.Drift);
        }

        [Test]
        public void ReportsNoInputWhileBlocked()
        {
            var gate = new GatedInputSource(new Fake(), () => true);
            Assert.AreEqual(Vector2.zero, gate.Move);
            Assert.IsFalse(gate.InteractPressed);
            Assert.IsFalse(gate.Drift);
        }
    }
}
