using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Lot;
using UncannyValet.Valet;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Mud patches: a driven car and a game-driven car are slowed and recover, the valet on foot is not, and mud never makes a washed car Muddy.</summary>
    public sealed class MudPatchPlayModeTests
    {
        private const float MudLaneX = 0f;
        private const float ClearLaneX = 30f;
        private const float PatchSize = 4f;
        private const int PatchCount = 6;
        private const float FirstPatchY = 6f;

        private sealed class WalkInput : IInputSource
        {
            public Vector2 Move => Vector2.up;
            public bool InteractPressed => false;
            public bool Drift => false;
        }

        private GameObject _factoryObject;
        private GameObject _lot;
        private CarFactory _factory;
        private CarDefinition _definition;
        private TuningStore _tuning;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            var bus = new EventBus();
            Services.Register<IEventBus>(bus);
            _tuning = new TuningStore(bus);
            MudRows.SeedDefaults(_tuning);
            Services.Register<ITuning>(_tuning);
            _definition = ScriptableObject.CreateInstance<CarDefinition>();
            _factoryObject = new GameObject("Cars");
            _factoryObject.SetActive(false);
            _factory = _factoryObject.AddComponent<CarFactory>();
            Set("_defaultDefinition", _definition);
            Set("_flash", ScriptableObject.CreateInstance<CarFlashConfig>());
            Set("_handling", ScriptableObject.CreateInstance<CarHandlingConfig>());
            _factoryObject.SetActive(true);
            _lot = new GameObject("Lot");
            var patches = new List<MudPatch>();
            for (var i = 0; i < PatchCount; i++) patches.Add(new MudPatch(new Vector2(MudLaneX, FirstPatchY + i * PatchSize * 0.9f), PatchSize));
            MudPatchBuilder.Build(_lot.transform, patches);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var car in Object.FindObjectsByType<CarDriver>(FindObjectsSortMode.None)) Object.DestroyImmediate(car.gameObject);
            Object.DestroyImmediate(_lot);
            Object.DestroyImmediate(_factoryObject);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator ADrivenCarCrossingMudLosesSpeedAndGripThenRecovers()
        {
            var muddy = Spawn(1, new Vector2(MudLaneX, 0f));
            var clear = Spawn(2, new Vector2(ClearLaneX, 0f));
            yield return null; // Start resolves the patches
            muddy.SetDriver(true);
            clear.SetDriver(true);
            var effect = muddy.GetComponent<CarMudEffect>();
            var sawMud = false;
            var gripScale = 1f;
            for (var t = 0f; t < 2.2f; t += Time.fixedDeltaTime)
            {
                muddy.SetInput(1f, 0f);
                clear.SetInput(1f, 0f);
                yield return new WaitForFixedUpdate();
                if (effect.Weight > 0f)
                {
                    sawMud = true;
                    gripScale = muddy.Modifiers.Apply(new DriveInput(1f, 0f, false)).GripLossScale;
                }
            }
            Assert.IsTrue(sawMud, "the car drove into a patch");
            Assert.Less(gripScale, 1f, "drift grip loss is reduced while in mud");
            Assert.Less(muddy.transform.position.y, clear.transform.position.y - 1f, "the muddy car covered less ground");
            // Out of the last patch (about 28 units up) the effect has ended and the car speeds up again.
            muddy.transform.position = new Vector3(MudLaneX + 15f, 40f, 0f);
            for (var t = 0f; t < 0.6f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
            Assert.AreEqual(0f, effect.Weight, "0.3 s after leaving the effect is gone");
            Assert.AreEqual(1f, effect.SpeedScale);
        }

        [UnityTest]
        public IEnumerator AGameDrivenCarIsSlowedByMudToo()
        {
            var muddy = Spawn(1, new Vector2(MudLaneX, 0f));
            var clear = Spawn(2, new Vector2(ClearLaneX, 0f));
            yield return null;
            muddy.GetComponent<CarAutoDrive>().BeginDeparting(_definition, null);
            clear.GetComponent<CarAutoDrive>().BeginDeparting(_definition, null);
            for (var t = 0f; t < 2f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
            Assert.Less(muddy.transform.position.y, clear.transform.position.y - 0.5f, "the departing car in mud falls behind");
        }

        [UnityTest]
        public IEnumerator TheValetOnFootWalksAtTheSameSpeedInMud()
        {
            var walker = new GameObject("Valet", typeof(Rigidbody2D), typeof(OnFootMover));
            walker.GetComponent<Rigidbody2D>().gravityScale = 0f;
            walker.transform.position = new Vector2(MudLaneX, FirstPatchY);
            walker.GetComponent<OnFootMover>().SetInput(new WalkInput());
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            var speed = walker.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
            Assert.IsTrue(Services.Get<IMudPatches>().IsInside(walker.transform.position), "standing in mud");
            Assert.AreEqual(4f, speed, 1e-3f);
            Assert.IsNull(walker.GetComponent<CarMudEffect>());
            Object.DestroyImmediate(walker);
        }

        [UnityTest]
        public IEnumerator AWashedCarDrivenThroughMudStaysClean()
        {
            Services.Register<IModifierCatalog>(ModifierDefaults.CreateCatalog());
            var car = Spawn(1, new Vector2(MudLaneX, 0f));
            var overlay = car.GetComponentInChildren<CarMudOverlay>(true);
            overlay.gameObject.SetActive(true);
            yield return null;
            Services.Get<IEventBus>().Publish(new ModifierCleared(car.Id, ModifierId.Muddy));
            Assert.IsFalse(overlay.gameObject.activeSelf, "washed");
            car.SetDriver(true);
            for (var t = 0f; t < 2.2f; t += Time.fixedDeltaTime)
            {
                car.SetInput(1f, 0f);
                yield return new WaitForFixedUpdate();
            }
            Assert.Greater(car.transform.position.y, FirstPatchY, "it drove through the patches");
            Assert.IsFalse(overlay.gameObject.activeSelf, "mud did not make it Muddy again");
        }

        private CarDriver Spawn(int id, Vector2 position) => (CarDriver)_factory.Spawn(_definition, new CarId(id), position, 0f, default, false);

        private void Set(string field, Object value)
            => typeof(CarFactory).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_factory, value);
    }
}
