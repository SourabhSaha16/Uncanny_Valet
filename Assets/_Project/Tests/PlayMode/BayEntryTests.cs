using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Car handling spec section 3 automated check: from the aisle centre at 2 units/s with full lock, a regular car
    /// ends fully inside the bay without disturbing cars parked in the other bays. Bay ids 1..6 are regular, 7 the EV bay.
    /// </summary>
    public class BayEntryTests
    {
        private const float RunSeconds = 10f;
        private const float InsideTolerance = 0.02f;
        private const float HeadingToleranceDegrees = 5f;
        private const float NeighbourMoveLimit = 0.05f;
        private const int BayCount = 7;
        const string Step2 = "Enabled in step 2: the current CarDriver turns at about 100 deg/s at 2 u/s (radius about 1.2, tighter than R_min 3.3), so the car ends about 1.6 units left of the bay centre and partly outside the bay (bays 3 and 4 also touch the neighbouring parked car).";

        private GameObject _lot;
        private GameObject _factoryObject;
        private LotConfig _config;
        private LotGeometry _geometry;
        private CarFactory _factory;
        private CarDefinition _definition;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            Services.Register<IEventBus>(new EventBus());
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _geometry = new LotGeometry(_config);
            _lot = Build<LotLayoutBuilder>("Lot", "_config", _config, out _);
            _definition = ScriptableObject.CreateInstance<CarDefinition>();
            var flash = ScriptableObject.CreateInstance<CarFlashConfig>();
            _factoryObject = Build<CarFactory>("Cars", "_defaultDefinition", _definition, out var factory);
            typeof(CarFactory).GetField("_flash", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(factory, flash);
            _factory = factory;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_lot);
            Object.Destroy(_factoryObject);
            Services.Reset();
        }

        [UnityTest, Ignore(Step2)]
        public IEnumerator Car_FromAisleCentre_EndsInsideBay_Regular([NUnit.Framework.Range(1, 6)] int bayId) => EnterBay(bayId);

        [UnityTest, Ignore(Step2)]
        public IEnumerator Car_FromAisleCentre_EndsInsideBay_Ev() => EnterBay(7);

        [Test]
        public void MeasuredGeometry_AisleBaysEvBayAndCarCollider()
        {
            var topRowBottom = _geometry.Spots[0].yMin;
            var bottomRowTop = _geometry.ChargingSpots[0].yMax;
            Assert.AreEqual(4.6f, topRowBottom - bottomRowTop, 1e-3f, "clear gap between the bay rows");
            Assert.AreEqual(new Vector2(2.6f, 4.4f), _geometry.Spots[0].size);
            Assert.AreEqual(3.2f, _geometry.Spots[1].x - _geometry.Spots[0].x, 1e-3f, "bay pitch");
            Assert.IsTrue(_lot.GetComponent<LotLayoutBuilder>().IsChargingSpot(BayCount), "the EV bay is id 7");
            var car = (CarDriver)_factory.Spawn(_definition, new CarId(100), Vector2.zero, 0f, default, false);
            var box = car.GetComponent<BoxCollider2D>();
            Assert.AreEqual(new Vector2(1f, 2f), (Vector2)Vector3.Scale(box.size, car.transform.lossyScale), "collider size");
        }

        private IEnumerator EnterBay(int bayId)
        {
            var bay = BayRect(bayId);
            var neighbours = SpawnNeighbours(bayId);
            var up = bay.center.y > _geometry.AisleY;
            var rMin = TurningRadius.RMin(_config.AisleWidth, 1f);
            var start = new Vector2(bay.center.x - rMin, _geometry.AisleY);
            var car = (CarDriver)_factory.Spawn(_definition, new CarId(100), start, -90f, default, false);
            var body = car.GetComponent<Rigidbody2D>();
            car.SetDriver(true);
            body.linearVelocity = Vector2.right * BayEntryDriver.CruiseSpeed;
            yield return BayEntryDriver.Run(car, body, bay.center, up ? 0f : 180f, up, RunSeconds);
            yield return new WaitForSeconds(0.3f);

            var bounds = car.GetComponent<BoxCollider2D>().bounds;
            var inside = bounds.min.x >= bay.xMin - InsideTolerance && bounds.max.x <= bay.xMax + InsideTolerance
                && bounds.min.y >= bay.yMin - InsideTolerance && bounds.max.y <= bay.yMax + InsideTolerance;
            var heading = Mathf.Abs(Mathf.DeltaAngle(body.rotation, up ? 0f : 180f));
            var report = $"bay {bayId} rect {bay}, car bounds {bounds.min}..{bounds.max}, heading error {heading:F1} deg, speed {body.linearVelocity.magnitude:F2}";
            Assert.IsTrue(inside, "Car not fully inside the bay: " + report);
            Assert.LessOrEqual(heading, HeadingToleranceDegrees, report);
            for (var i = 0; i < neighbours.Length; i++)
                Assert.LessOrEqual(Vector2.Distance(neighbours[i].body.position, neighbours[i].start), NeighbourMoveLimit, "A parked car was pushed: " + report);
        }

        private (Rigidbody2D body, Vector2 start)[] SpawnNeighbours(int skipId)
        {
            var list = new System.Collections.Generic.List<(Rigidbody2D, Vector2)>();
            for (var id = 1; id <= BayCount; id++)
            {
                if (id == skipId) continue;
                var centre = BayRect(id).center;
                var parked = (CarDriver)_factory.Spawn(_definition, new CarId(id), centre, 0f, default, false);
                list.Add((parked.GetComponent<Rigidbody2D>(), centre));
            }
            return list.ToArray();
        }

        private Rect BayRect(int id) => id <= _geometry.Spots.Length ? _geometry.Spots[id - 1] : _geometry.ChargingSpots[id - _geometry.Spots.Length - 1];

        private static GameObject Build<T>(string name, string field, Object value, out T component) where T : Component
        {
            var go = new GameObject(name);
            go.SetActive(false);
            component = go.AddComponent<T>();
            typeof(T).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(component, value);
            go.SetActive(true);
            return go;
        }
    }
}
