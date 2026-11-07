using NUnit.Framework;
using UncannyValet.Bootstrap;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    public class CameraFollowTests
    {
        [Test]
        public void SetWorldBounds_BeforeAwake_DoesNotThrow()
        {
            // Edit mode does not run Awake, which is the order GameBootstrap.Awake can meet in a build.
            var go = new GameObject("Cam", typeof(Camera));
            var follow = go.AddComponent<CameraFollow>();
            Assert.DoesNotThrow(() => follow.SetWorldBounds(new Rect(-10f, -5f, 20f, 10f)));
            Assert.IsTrue(go.GetComponent<Camera>().orthographic);
            Object.DestroyImmediate(go);
        }
    }
}
