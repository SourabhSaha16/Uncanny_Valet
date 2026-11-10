using System;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using static UncannyValet.Tests.Director.PickerTestKit;

namespace UncannyValet.Tests.Director
{
    public class ArchetypePickerTests
    {
        private const int BigBudget = 1000;

        private DirectorSettings _settings;

        [SetUp]
        public void SetUp() => _settings = new DirectorSettings();

        private ArchetypePicker Picker(int seed) => PickerTestKit.Picker(_settings, seed);

        [Test]
        public void LockedStationsNeverEnterTheDraw()
        {
            AssertNever(Stations(false, false), a => a != CarArchetype.Plain, "no stations");
            AssertNever(Stations(false, true), a => ArchetypeKinds.NeedsWash(a), "no wash");
            AssertNever(Stations(true, false), a => ArchetypeKinds.NeedsCharge(a), "no bays");
        }

        private void AssertNever(SnapshotBuilderForTests lot, Func<CarArchetype, bool> forbidden, string why)
        {
            for (var seed = 1; seed <= 30; seed++)
            {
                var picker = Picker(seed);
                for (var i = 0; i < 60; i++)
                    Assert.IsFalse(forbidden(picker.Pick(Request(lot)).Archetype), why);
            }
        }

        [Test]
        public void ReliefGivesOnlyPlainCarsAndNoRule()
        {
            var lot = Stations(true, true, StationSlotState.Blocked);
            var picker = Picker(7);
            for (var i = 0; i < 100; i++)
            {
                var pick = picker.Pick(Request(lot, relief: true, dilemmas: true));
                Assert.AreEqual(CarArchetype.Plain, pick.Archetype);
                Assert.IsFalse(pick.HasRule);
            }
        }

        [Test]
        public void StreakLimitHoldsEvenWhenDirtyAndEvDominate()
        {
            var heavy = new ArchetypeWeights(1f, 50f, 50f, 50f);
            var lot = Stations(true, true);
            for (var seed = 1; seed <= 20; seed++)
            {
                var picker = Picker(seed);
                var dirtyRun = 0;
                var evRun = 0;
                for (var i = 0; i < 40; i++)
                {
                    var request = new PickRequest(lot.Build(), heavy, ScheduleReason.OnTarget, false, false, BigBudget);
                    var a = picker.Pick(request).Archetype;
                    dirtyRun = ArchetypeKinds.NeedsWash(a) ? dirtyRun + 1 : 0;
                    evRun = ArchetypeKinds.NeedsCharge(a) ? evRun + 1 : 0;
                    Assert.LessOrEqual(dirtyRun, 2, "seed " + seed + " pick " + i);
                    Assert.LessOrEqual(evRun, 2, "seed " + seed + " pick " + i);
                }
            }
        }

        [Test]
        public void SameSeedGivesTheSamePicksAndAnotherSeedDiffers()
        {
            var lot = Stations(true, true, StationSlotState.Blocked);
            var a = Sequence(3, lot);
            var b = Sequence(3, lot);
            var c = Sequence(4, lot);
            CollectionAssert.AreEqual(a, b);
            CollectionAssert.AreNotEqual(a, c);
        }

        private List<CarArchetype> Sequence(int seed, SnapshotBuilderForTests lot)
        {
            var picker = Picker(seed);
            var list = new List<CarArchetype>();
            for (var i = 0; i < 100; i++) list.Add(picker.Pick(Request(lot, dilemmas: true)).Archetype);
            return list;
        }
    }
}
