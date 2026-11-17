using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public sealed class SpotGeometryTests
    {
        [Test]
        public void KnownBayReturnsItsRectangle()
        {
            var geometry = new SpotGeometry();
            geometry.Set(3, new SpotBounds(new Vector2(1f, 2f), new Vector2(2.6f, 4.6f), 90f));
            Assert.IsTrue(geometry.TryGetSpot(3, out var bay));
            Assert.AreEqual(new Vector2(1f, 2f), bay.Center);
            Assert.AreEqual(new Vector2(2.6f, 4.6f), bay.Size);
            Assert.AreEqual(90f, bay.RotationDegrees);
        }

        [Test]
        public void UnknownBayReturnsFalse() => Assert.IsFalse(new SpotGeometry().TryGetSpot(7, out _));
    }
}
