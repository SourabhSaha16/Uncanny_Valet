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
    /// <summary>The two Vanaheim lot scenes as built: bays and troughs, mud patches, the crowd and traffic of each lot, and a muddy car washed at a trough.</summary>
    public sealed class VanaheimWorldPlayModeTests : VanaheimPlayBase
    {
        private const float SampleSeconds = 0.5f;
        private const int Samples = 8;
        private const float SettleSeconds = 1.5f;
        private const int WashMinutes = 16;
        private const int CarIdBase = 700;
        private const float StayMinutes = 600f;
        private const int SpawnAttempts = 120;

        [UnityTest]
        public IEnumerator TrainingLot_HasEightBays_OneTrough_NoMud_FourWalkers_AndTodaysTraffic()
        {
            yield return Play(TrainingPath);
            AssertBays(1);
            var tuning = Services.Get<ITuning>();
            Assert.AreEqual(4f, tuning.Get(TuningKey.PedestrianCount));
            Assert.AreEqual(0f, tuning.Get(TuningKey.PedestrianCurses), "pedestrian curses are off");
            Assert.IsNull(GameObject.Find("Mud Patches"), "no mud patches in the training lot");
            Assert.IsNotNull(GameObject.Find("VanaheimBarn")?.GetComponent<BoxCollider2D>(), "the barn is solid");
            Assert.AreEqual(1, Object.FindObjectsByType<StationZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length, "the trough is open");
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
        public IEnumerator CrazyLot_HasEightBays_ThreeTroughs_FiveMudPatches_EightWalkers_AndTodaysTraffic()
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
            Assert.AreEqual(3, Object.FindObjectsByType<StationZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length, "the three troughs are open");
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
        public IEnumerator ACarLeftInEachTrough_IsReportedAsWashing_ByThatTrough()
        {
            yield return Play(CrazyPath);
            var entered = new List<CarId>();
            Services.Get<IEventBus>().Subscribe<CarEnteredStation>(e => { if (e.Station == StationKind.Wash) entered.Add(e.Car); });
            var zones = Object.FindObjectsByType<StationZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var factory = Services.Get<ICarFactory>();
            for (var i = 0; i < zones.Length; i++) factory.Spawn(new CarId(CarIdBase + i), zones[i].transform.position, 0f);
            yield return new WaitForSeconds(SettleSeconds);
            for (var i = 0; i < zones.Length; i++) CollectionAssert.Contains(entered, new CarId(CarIdBase + i), "trough " + i);
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }

        [UnityTest]
        public IEnumerator AMuddyCar_IsWashedAtATrough_EndToEnd()
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
            var trough = Object.FindObjectsByType<StationZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)[0];
            factory.Spawn(ticket.CarId, trough.transform.position, 0f);
            yield return new WaitForSeconds(SettleSeconds);
            Assert.IsTrue(trough.Holds(ticket.CarId), "the car settled in the trough");
            for (var m = 0; m < WashMinutes; m++) bus.Publish(new ShiftTick(m));
            Assert.IsTrue(cleared.Exists(e => e.Car == ticket.CarId && e.Modifier == ModifierId.Muddy), "the trough washed Muddy off");
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }

        private static void AssertBays(int troughs)
        {
            var layout = Services.Get<ILotLayout>();
            Assert.AreEqual(8, layout.RegularSpotCount);
            Assert.AreEqual(0, layout.ChargingSpotCount, "no EV bay");
            Assert.AreEqual(8, layout.SpotIds.Count, "all eight bays are open");
            Assert.IsTrue(layout.HasWashStation);
            Assert.AreEqual(troughs, layout.WashStationCount);
            Assert.AreEqual(troughs, Object.FindObjectsByType<StationZone>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length, "one station per trough");
        }
    }
}
