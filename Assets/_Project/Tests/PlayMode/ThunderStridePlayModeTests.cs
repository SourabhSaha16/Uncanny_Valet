using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
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
    /// <summary>Thunder stride: the on-foot dash stops at a wall and passes pedestrians; the car boost is faster and still takes damage; the cooldown blocks a second use.</summary>
    public class ThunderStridePlayModeTests
    {
        private sealed class StubInput : MonoBehaviour, IInputSource
        {
            public Vector2 Move { get; set; }
            public bool InteractPressed => false;
            public bool Drift => false;
        }

        private sealed class DefaultsView : IPowerView
        {
            public bool TryGetState(StoryPower power, out PowerState state) { state = default; return false; }

            public bool TryGetValues(StoryPower power, int level, out PowerLevelValues values)
            {
                values = PowerDefaults.Values(power, level);
                return level >= 1 && level <= 2;
            }
        }

        private IEventBus _bus;
        private readonly List<GameObject> _made = new List<GameObject>();
        private GameObject _factoryObject;
        private CarFactory _factory;
        private CarDefinition _definition;
        private ThunderBoostLink _link;
        private readonly List<CarImpact> _impacts = new List<CarImpact>();

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            Services.Register<IEventBus>(_bus);
            Services.Register<IPowerView>(new DefaultsView());
            _bus.Subscribe<CarImpact>(_impacts.Add);
        }

        [TearDown]
        public void TearDown()
        {
            _link?.Dispose();
            _link = null;
            _bus.Unsubscribe<CarImpact>(_impacts.Add);
            _impacts.Clear();
            foreach (var go in _made) if (go != null) Object.DestroyImmediate(go);
            _made.Clear();
            if (_factoryObject != null) Object.DestroyImmediate(_factoryObject);
            Services.Reset();
        }

        private GameObject Made(GameObject go) { _made.Add(go); return go; }

        private (GameObject valet, Rigidbody2D body, StubInput input) MakeValet()
        {
            var go = Made(new GameObject("Valet"));
            go.SetActive(false);
            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            go.AddComponent<CircleCollider2D>().radius = 0.4f;
            var input = go.AddComponent<StubInput>();
            var mover = go.AddComponent<OnFootMover>();
            var interactor = go.AddComponent<Interactor>();
            var seat = go.AddComponent<SeatController>();
            go.AddComponent<ThunderDash>();
            Field(seat, "_mover", mover);
            Field(seat, "_interactor", interactor);
            Field(seat, "_bodyRenderers", new Renderer[0]);
            Field(seat, "_bodyColliders", new Collider2D[0]);
            go.SetActive(true);
            return (go, body, input);
        }

        private static void Field(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        private GameObject Solid(Vector2 at, Vector2 size, int layer = 0)
        {
            var go = Made(new GameObject("Solid") { layer = layer });
            go.transform.position = at;
            go.AddComponent<BoxCollider2D>().size = size;
            return go;
        }

        private IEnumerator Dash(Rigidbody2D body, StubInput input, int level)
        {
            input.Move = Vector2.right;
            yield return new WaitForFixedUpdate();
            _bus.Publish(new PowerActivated(StoryPower.ThunderStride, level));
            input.Move = Vector2.zero; // the dash direction was taken at the press; walking adds nothing afterwards
            for (var t = 0f; t < 0.6f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator TheDashCoversTheLevelDistance_AndPassesAPedestrianLayerCollider([Values(1, 2)] int level)
        {
            var (_, body, input) = MakeValet();
            Solid(new Vector2(2.5f, 0f), new Vector2(0.6f, 2f), PhysicsLayers.Pedestrian);
            yield return null; // Start of the valet components runs before the first Update
            yield return Dash(body, input, level);
            var expected = level == 1 ? 5f : 8f;
            Assert.AreEqual(expected, body.position.x, 0.4f, "went the full distance through the pedestrian");
            Assert.AreEqual(0f, body.position.y, 0.1f);
        }

        [UnityTest]
        public IEnumerator TheDashStopsAtAWall_AndPedestriansBlockAgainAfterwards()
        {
            var (go, body, input) = MakeValet();
            Solid(new Vector2(4f, 0f), new Vector2(1f, 4f));
            yield return null; // Start of the valet components runs before the first Update
            yield return Dash(body, input, 2);
            Assert.Less(body.position.x, 3.3f, "stopped at the wall face (3.5) minus the valet's radius");
            Assert.Greater(body.position.x, 2.8f, "a plain walk would only have reached about 2.4");
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(go.layer, PhysicsLayers.Pedestrian), "pedestrian collisions are back");
        }

        private void MakeFactory()
        {
            _definition = ScriptableObject.CreateInstance<CarDefinition>();
            _factoryObject = new GameObject("Cars");
            _factoryObject.SetActive(false);
            _factory = _factoryObject.AddComponent<CarFactory>();
            Field(_factory, "_defaultDefinition", _definition);
            Field(_factory, "_flash", ScriptableObject.CreateInstance<CarFlashConfig>());
            Field(_factory, "_handling", ScriptableObject.CreateInstance<CarHandlingConfig>());
            _factoryObject.SetActive(true);
            _link = new ThunderBoostLink(_bus, _factory, () => Services.TryGet<IPowerView>(out var v) ? v : null);
        }

        private IEnumerator Drive(bool boosted, float wallY, float seconds, List<float> topSpeed)
        {
            var car = (CarDriver)_factory.Spawn(_definition, new CarId(boosted ? 2 : 1), new Vector2(0f, -20f), 0f, default, false);
            _made.Add(car.gameObject);
            car.SetDriver(true);
            if (wallY < 100f) Solid(new Vector2(0f, wallY), new Vector2(20f, 1f));
            if (boosted) _bus.Publish(new PowerActivated(StoryPower.ThunderStride, 1));
            var top = 0f;
            for (var t = 0f; t < seconds; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                car.SetInput(1f, 0f);
                top = Mathf.Max(top, car.Speed);
            }
            topSpeed.Add(top);
            _bus.Publish(new PowerEnded(StoryPower.ThunderStride));
            Object.DestroyImmediate(car.gameObject);
        }

        [UnityTest]
        public IEnumerator ABoostedCarReachesAHigherSpeed()
        {
            MakeFactory();
            var tops = new List<float>();
            yield return Drive(false, 1000f, 3f, tops);
            yield return Drive(true, 1000f, 3f, tops);
            Assert.Greater(tops[1], tops[0] * 1.2f, "boosted " + tops[1] + " vs plain " + tops[0]);
        }

        [UnityTest]
        public IEnumerator ABoostedCarStillTakesDamageInAWallHit_HarderThanAPlainOne()
        {
            MakeFactory();
            var damaged = new List<CarDamaged>();
            _bus.Subscribe<CarDamaged>(damaged.Add);
            var tops = new List<float>();
            yield return Drive(false, 0f, 5f, tops);
            var plainForce = 0f;
            foreach (var i in _impacts) plainForce = Mathf.Max(plainForce, i.Force);
            _impacts.Clear();
            damaged.Clear();
            yield return Drive(true, 0f, 5f, tops);
            var boostedForce = 0f;
            foreach (var i in _impacts) boostedForce = Mathf.Max(boostedForce, i.Force);
            _bus.Unsubscribe<CarDamaged>(damaged.Add);
            Assert.IsTrue(damaged.Exists(d => d.Delta01 > 0f), "the boosted hit damages the car");
            Assert.Greater(boostedForce, plainForce, "a faster hit hurts more");
        }

        [UnityTest]
        public IEnumerator TheCooldownBlocksASecondUseUntilItHasRun()
        {
            var power = new ThunderStridePower(() => false);
            var stack = new PowerStack(_bus, new IPower[] { power }, new PowerCooldowns());
            var bucket = new Bucket { Open = 3 };
            var values = new PowerLevelValues(0.6f, 0f, 5f, 1.5f, 2f, 0.1f);
            Assert.AreEqual(FireResult.Started, stack.TryFire(StoryPower.ThunderStride, 1, values, bucket));
            var until = Time.unscaledTime + 0.3f;
            while (Time.unscaledTime < until) { stack.Tick(Time.unscaledDeltaTime); yield return null; }
            Assert.AreEqual(FireResult.NotReady, stack.TryFire(StoryPower.ThunderStride, 1, values, bucket));
            Assert.AreEqual(1, bucket.Spent, "the blocked second use spends nothing");
            until = Time.unscaledTime + 0.8f;
            while (Time.unscaledTime < until) { stack.Tick(Time.unscaledDeltaTime); yield return null; }
            Assert.AreEqual(FireResult.Started, stack.TryFire(StoryPower.ThunderStride, 1, values, bucket));
        }

        private sealed class Bucket : IPowerBucket
        {
            public int Open;
            public int Spent;
            public bool CanSpend => Open > 0;
            public bool TrySpend() { if (Open <= 0) return false; Open--; Spent++; return true; }
        }
    }
}
