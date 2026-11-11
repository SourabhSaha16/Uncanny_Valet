using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Valet;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Speed boost barge-through (car_handling_spec.md row 8): a boosted valet walks through a pedestrian and the pedestrian is told; otherwise it blocks.</summary>
    public class ValetBargeTests
    {
        private const float Speed = 4f;
        private const float WalkerX = 2f;
        private const float RunSeconds = 1.2f;

        private sealed class FakeBoost : IPowerBoost
        {
            public bool IsBoosted { get; set; }
            public float WalkMultiplier => 1.4f;
            public float Remaining => 4f;
        }

        private sealed class FakeInput : IInputSource
        {
            public Vector2 Move => Vector2.right;
            public bool InteractPressed => false;
            public bool Drift => false;
        }

        private GameObject _valet;
        private GameObject _walker;
        private FakeBoost _boost;
        private IEventBus _bus;
        private readonly List<ValetBargedPedestrian> _barges = new List<ValetBargedPedestrian>();

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _bus = new EventBus();
            _boost = new FakeBoost();
            Services.Register(_bus);
            Services.Register<IPowerBoost>(_boost);
            _bus.Subscribe<ValetBargedPedestrian>(_barges.Add);
            _walker = new GameObject("walker") { layer = PhysicsLayers.Pedestrian };
            _walker.transform.position = new Vector3(WalkerX, 0f, 0f);
            _walker.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            _walker.AddComponent<CircleCollider2D>().radius = 0.22f;
            _valet = new GameObject("valet");
            var body = _valet.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            _valet.AddComponent<CircleCollider2D>().radius = 0.3f;
            _valet.AddComponent<OnFootMover>().SetInput(new FakeInput());
        }

        [TearDown]
        public void TearDown()
        {
            _bus.Unsubscribe<ValetBargedPedestrian>(_barges.Add);
            _barges.Clear();
            Object.DestroyImmediate(_valet);
            Object.DestroyImmediate(_walker);
            Services.Reset();
        }

        [UnityTest]
        public IEnumerator NotBoosted_ThePedestrianBlocksTheValet_NobodyIsBarged()
        {
            yield return new WaitForSeconds(RunSeconds);
            Assert.Less(_valet.transform.position.x, WalkerX, "blocked by the walker");
            Assert.AreEqual(0, _barges.Count);
        }

        [UnityTest]
        public IEnumerator Boosted_TheValetRunsThroughAndTheWalkerIsToldOnce()
        {
            _boost.IsBoosted = true;
            yield return new WaitForSeconds(RunSeconds);
            Assert.Greater(_valet.transform.position.x, WalkerX + 0.5f, "walked through");
            Assert.AreEqual(1, _barges.Count, "one event despite several overlapping steps");
            Assert.AreEqual(_walker.GetInstanceID(), _barges[0].Pedestrian.Value);
            Assert.Greater(_barges[0].PushX, 0f, "pushed away from the valet");
        }

        [UnityTest]
        public IEnumerator Boosted_WalksFasterByTheMultiplier()
        {
            _walker.SetActive(false);
            _boost.IsBoosted = true;
            yield return new WaitForSeconds(0.5f);
            var rate = _valet.GetComponent<Rigidbody2D>().linearVelocity.x;
            Assert.AreEqual(Speed * 1.4f, rate, 0.05f);
        }

        [UnityTest]
        public IEnumerator WhenTheBoostEnds_PedestriansAreSolidAgain()
        {
            _walker.SetActive(false);
            _boost.IsBoosted = true;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            var collider = _valet.GetComponent<CircleCollider2D>();
            Assert.AreNotEqual(0, collider.excludeLayers.value & PhysicsLayers.PedestrianMask);
            _boost.IsBoosted = false;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(0, collider.excludeLayers.value & PhysicsLayers.PedestrianMask);
        }
    }
}
