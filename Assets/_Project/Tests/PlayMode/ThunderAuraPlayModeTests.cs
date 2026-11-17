using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UncannyValet.Valet;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>The Thunder stride glow tints and lights the valet for the whole boost and leaves nothing behind when it ends.</summary>
    public class ThunderAuraPlayModeTests
    {
        private sealed class View : IPowerView
        {
            public bool TryGetState(StoryPower power, out PowerState state) { state = default; return false; }

            public bool TryGetValues(StoryPower power, int level, out PowerLevelValues values)
            {
                values = PowerDefaults.Values(power, level);
                return level >= 1 && level <= 2;
            }
        }

        private sealed class StubInput : MonoBehaviour, IInputSource
        {
            public Vector2 Move => Vector2.zero;
            public bool InteractPressed => false;
            public bool Drift => false;
        }

        private IEventBus _bus;
        private GameObject _valet;
        private SpriteRenderer _sprite;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            Services.Register<IPowerView>(new View());
            _valet = new GameObject("Valet");
            _valet.SetActive(false);
            var body = _valet.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            _valet.AddComponent<StubInput>();
            var mover = _valet.AddComponent<OnFootMover>();
            var interactor = _valet.AddComponent<Interactor>();
            var seat = _valet.AddComponent<SeatController>();
            _valet.AddComponent<ThunderDash>();
            Field(seat, "_mover", mover);
            Field(seat, "_interactor", interactor);
            Field(seat, "_bodyRenderers", new Renderer[0]);
            Field(seat, "_bodyColliders", new Collider2D[0]);
            var child = new GameObject("Body");
            child.transform.SetParent(_valet.transform, false);
            _sprite = child.AddComponent<SpriteRenderer>();
            _valet.SetActive(true);
        }

        [TearDown]
        public void TearDown()
        {
            if (_valet != null) Object.DestroyImmediate(_valet);
            Services.Reset();
        }

        private static void Field(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        private static SpriteRenderer Glow()
        {
            var go = GameObject.Find("Thunder glow");
            return go != null ? go.GetComponent<SpriteRenderer>() : null;
        }

        [UnityTest]
        public IEnumerator TheValetIsTintedAndGlowsWhileTheBoostRunsAndIsCleanWhenItEnds()
        {
            yield return null;
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            yield return null;
            yield return null;
            Assert.IsTrue(_sprite.HasPropertyBlock(), "tinted");
            Assert.IsNotNull(Glow());
            Assert.IsTrue(Glow().enabled, "glow on");
            _bus.Publish(new PowerEnded(StoryPower.ThunderStride));
            yield return null;
            Assert.IsFalse(_sprite.HasPropertyBlock(), "tint removed");
            Assert.IsFalse(Glow().enabled, "glow off");
        }

        [UnityTest]
        public IEnumerator TheShiftEndingAlsoClearsTheGlow()
        {
            yield return null;
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 2));
            yield return null;
            yield return null;
            Assert.IsTrue(_sprite.HasPropertyBlock());
            _bus.Publish(new ShiftEnded());
            yield return null;
            Assert.IsFalse(_sprite.HasPropertyBlock());
            Assert.IsFalse(Glow().enabled);
        }

        [UnityTest]
        public IEnumerator DestroyingTheValetRemovesTheGlowObject()
        {
            yield return null;
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            yield return null;
            Assert.IsNotNull(Glow());
            Object.Destroy(_valet);
            yield return null;
            yield return null;
            Assert.IsNull(Glow(), "no leaked glow");
        }
    }
}
