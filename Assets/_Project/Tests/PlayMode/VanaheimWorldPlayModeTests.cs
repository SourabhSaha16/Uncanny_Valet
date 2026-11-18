using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Modifiers;
using UncannyValet.Lot;
using UncannyValet.Patrons;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>The two Vanaheim lot scenes as built: bays and wash bays, mud patches, the crowd and traffic of each lot, and a muddy car washed at a wash bay.</summary>
    public sealed class VanaheimWorldPlayModeTests : VanaheimPlayBase
    {
        private const float SampleSeconds = 0.5f;
        private const int Samples = 8;
        private const float SettleSeconds = 1.5f;
        private const int WashMinutes = 16;
        private const int CarIdBase = 700;
        private const float StayMinutes = 600f;
        private const int SpawnAttempts = 120;
        private const float FenceApproach = 6f;
        private const float FenceRunSeconds = 3f;
        private const float FenceSpeed = 8f;

        [UnityTest]
        public IEnumerator TrainingLot_HasEightBays_OneWashBay_AWheatFieldOfMud_FourWalkers_AndTodaysTraffic()
        {
            yield return Play(TrainingPath);
            AssertBays(1);
            var tuning = Services.Get<ITuning>();
            Assert.AreEqual(4f, tuning.Get(TuningKey.PedestrianCount));
            Assert.AreEqual(0f, tuning.Get(TuningKey.PedestrianCurses), "pedestrian curses are off");
            var field = GameObject.Find("Mud Patches");
            Assert.IsNotNull(field, "the wheat field is a mud patch");
            Assert.AreEqual(1, field.transform.childCount);
            Assert.IsTrue(Services.Get<IMudPatches>().IsInside(field.transform.GetChild(0).position), "a car in the wheat is slowed");
            Assert.IsNotNull(GameObject.Find("VanaheimBarn")?.GetComponent<BoxCollider2D>(), "the barn is solid");
            Assert.AreEqual(1, Object.FindObjectsByType<StationZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length, "the wash bay is open");
            var most = 0;
            var cars = 0;
            for (var i = 0; i < Samples; i++)
            {
                yield return new WaitForSeconds(SampleSeconds);
                most = Mathf.Max(most, VisibleAmbientWalkers());
                cars = Mathf.Max(cars, ActiveAmbientCars());
            }
            Assert.LessOrEqual(most, 4, "no more than four walkers");
            Assert.GreaterOrEqual(most, 1, "the villagers are out");
            Assert.Greater(cars, 0, "the road has ambient traffic");
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }

        [UnityTest]
        public IEnumerator TheFence_IsSolid_AndABumpingCarTakesDamage()
        {
            yield return Play(TrainingPath);
            var fence = GameObject.Find("VanaheimFence");
            Assert.IsNotNull(fence?.GetComponent<BoxCollider2D>(), "the fence is solid");
            Assert.IsNotNull(GameObject.Find("VanaheimHayBale")?.GetComponent<BoxCollider2D>(), "the hay bales are solid");
            Assert.IsNull(GameObject.Find("VanaheimWheat")?.GetComponent<Collider2D>(), "the wheat is not solid");
            var damaged = new List<CarDamaged>();
            Services.Get<IEventBus>().Subscribe<CarDamaged>(damaged.Add);
            var car = Services.Get<ICarFactory>().Spawn(new CarId(CarIdBase), (Vector2)fence.transform.position + Vector2.down * FenceApproach, 0f);
            var body = car.Transform.GetComponent<Rigidbody2D>(); // not seated: a seated car would be steered by the valet's keyboard
            for (var t = 0f; t < FenceRunSeconds; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                body.linearVelocity = Vector2.up * FenceSpeed;
            }
            Services.Get<IEventBus>().Unsubscribe<CarDamaged>(damaged.Add);
            Assert.IsTrue(damaged.Exists(d => d.Car == car.Id && d.Damage01 > 0f), "the car took damage from the fence");
        }

        [UnityTest]
        public IEnumerator CrazyLot_HasSolidStallsAndBanners()
        {
            yield return Play(CrazyPath);
            Assert.IsNotNull(GameObject.Find("VanaheimStall")?.GetComponent<BoxCollider2D>(), "the stalls are solid");
            Assert.IsNotNull(GameObject.Find("VanaheimBanner")?.GetComponent<BoxCollider2D>(), "the banners are solid");
        }

        [UnityTest]
        public IEnumerator CrazyLot_HasEightBays_ThreeWashBays_FiveMudPatches_EightWalkers_AndTodaysTraffic()
        {
            yield return Play(CrazyPath);
            AssertBays(3);
            var tuning = Services.Get<ITuning>();
            Assert.AreEqual(8f, tuning.Get(TuningKey.PedestrianCount));
            Assert.AreEqual(0f, tuning.Get(TuningKey.PedestrianCurses), "pedestrian curses are off");
            var mud = GameObject.Find("Mud Patches");
            Assert.IsNotNull(mud, "the crazy lot has mud");
            Assert.AreEqual(5, mud.transform.childCount);
            Assert.IsTrue(Services.TryGet<IMudPatches>(out _), "cars feel the patches");
            Assert.AreEqual(3, Object.FindObjectsByType<StationZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length, "the three wash bays are open");
            var most = 0;
            var cars = 0;
            for (var i = 0; i < Samples; i++)
            {
                yield return new WaitForSeconds(SampleSeconds);
                most = Mathf.Max(most, VisibleAmbientWalkers());
                cars = Mathf.Max(cars, ActiveAmbientCars());
            }
            Assert.LessOrEqual(most, 8);
            Assert.GreaterOrEqual(most, 3, "the feast guests are out");
            Assert.Greater(cars, 0, "the road has ambient traffic");
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }

        [UnityTest]
        public IEnumerator ACarLeftInEachWashBay_IsReportedAsWashing_ByThatWashBay()
        {
            yield return Play(CrazyPath);
            var entered = new List<CarId>();
            Services.Get<IEventBus>().Subscribe<CarEnteredStation>(e => { if (e.Station == StationKind.Wash) entered.Add(e.Car); });
            var zones = Object.FindObjectsByType<StationZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var factory = Services.Get<ICarFactory>();
            for (var i = 0; i < zones.Length; i++) factory.Spawn(new CarId(CarIdBase + i), zones[i].transform.position, 0f);
            yield return new WaitForSeconds(SettleSeconds);
            for (var i = 0; i < zones.Length; i++) CollectionAssert.Contains(entered, new CarId(CarIdBase + i), "wash bay " + i);
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }

        [UnityTest]
        public IEnumerator AMuddyCar_IsWashedAtAWashBay_EndToEnd()
        {
            yield return Play(CrazyPath);
            var bus = Services.Get<IEventBus>();
            var cleared = new List<ModifierCleared>();
            bus.Subscribe<ModifierCleared>(cleared.Add);
            var spawner = Object.FindFirstObjectByType<PatronSpawner>();
            var request = new PatronRequest(StayMinutes, 20, false, StayMinutes, ModifierSet.Of(ModifierId.Muddy));
            Ticket ticket = null;
            for (var i = 0; i < SpawnAttempts && ticket == null; i++) if (!spawner.TrySpawn(request, out ticket)) yield return null;
            Assert.IsNotNull(ticket, "no queue slot ever cleared for the test car");
            var factory = Services.Get<ICarFactory>();
            factory.Despawn(ticket.CarId);
            var washBay = Object.FindObjectsByType<StationZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)[0];
            factory.Spawn(ticket.CarId, washBay.transform.position, 0f);
            yield return new WaitForSeconds(SettleSeconds);
            Assert.IsTrue(washBay.Holds(ticket.CarId), "the car settled in the wash bay");
            for (var m = 0; m < WashMinutes; m++) bus.Publish(new ShiftTick(m));
            Assert.IsTrue(cleared.Exists(e => e.Car == ticket.CarId && e.Modifier == ModifierId.Muddy), "the wash bay washed Muddy off");
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }

        private static void AssertBays(int washBays)
        {
            var layout = Services.Get<ILotLayout>();
            Assert.AreEqual(8, layout.RegularSpotCount);
            Assert.AreEqual(0, layout.ChargingSpotCount, "no EV bay");
            Assert.AreEqual(8, layout.SpotIds.Count, "all eight bays are open");
            Assert.IsTrue(layout.HasWashStation);
            Assert.AreEqual(washBays, layout.WashStationCount);
            Assert.AreEqual(washBays, Object.FindObjectsByType<StationZone>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, "one station per wash bay");
        }
    }
}
