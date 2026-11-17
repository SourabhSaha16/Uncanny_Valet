using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class ThunderKickLinkTests
    {
        [Test]
        public void TheCameraKicksAlongTheDashDirectionUntilDisposed()
        {
            var bus = new EventBus();
            var kicks = new System.Collections.Generic.List<Vector2>();
            var link = new ThunderKickLink(bus, kicks.Add);
            bus.Publish(new ThunderStrideStarted(Vector2.zero, Vector2.left, false));
            bus.Publish(new ThunderStrideStarted(Vector2.zero, Vector2.up, true));
            link.Dispose();
            bus.Publish(new ThunderStrideStarted(Vector2.zero, Vector2.down, false));
            CollectionAssert.AreEqual(new[] { Vector2.left, Vector2.up }, kicks);
        }
    }
}
