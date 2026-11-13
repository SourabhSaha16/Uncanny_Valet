using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using static UncannyValet.Tests.Director.PickerTestKit;

namespace UncannyValet.Tests.Director
{
    public class ArchetypeBoostTests
    {
        private const float ShareSlack = 0.0001f;
        private const int BigBudget = 1000;

        private DirectorSettings _settings;

        [SetUp]
        public void SetUp() => _settings = new DirectorSettings();

        private ArchetypePicker Picker(int seed) => PickerTestKit.Picker(_settings, seed);

        [Test]
        public void TheMixStaysWithinTenPointsOfTheBaseShareOverABudgetShift()
        {
            var lot = Stations(true, true);
            var bands = new[] { ScheduleReason.OnTarget, ScheduleReason.UnderTarget, ScheduleReason.OverTarget };
            for (var seed = 1; seed <= 200; seed++)
            {
                var picker = Picker(seed);
                var band = bands[seed % bands.Length];
                for (var i = 0; i < Budget; i++) picker.Pick(Request(lot, band));
                AssertShare(picker, CarArchetype.Plain, 0.5f, seed);
                AssertShare(picker, CarArchetype.Muddy, 0.2f, seed);
                AssertShare(picker, CarArchetype.Ev, 0.2f, seed);
                AssertShare(picker, CarArchetype.MuddyEv, 0.1f, seed);
            }
        }

        private static void AssertShare(ArchetypePicker picker, CarArchetype a, float baseShare, int seed)
        {
            var share = picker.History.Count(a) / (float)Budget;
            Assert.GreaterOrEqual(share, baseShare - 0.1f - ShareSlack, a + " too rare, seed " + seed);
            Assert.LessOrEqual(share, baseShare + 0.1f + ShareSlack, a + " too common, seed " + seed);
        }

        [Test]
        public void ABoostedWinnerReportsItsRuleAndMultiplier()
        {
            var pick = FirstMuddyPick(out var seed);
            Assert.IsTrue(pick.HasRule, "seed " + seed);
            Assert.AreEqual(DilemmaKind.WashBlocked, pick.Rule);
            Assert.AreEqual(3f, pick.Multiplier);
        }

        // The first seed whose draw on a blocked-wash lot lands on Muddy (a x3 weight wins within a few seeds).
        private PickResult FirstMuddyPick(out int foundSeed)
        {
            var lot = Stations(true, true, StationSlotState.Blocked);
            for (var seed = 1; seed <= 30; seed++)
            {
                var pick = Picker(seed).Pick(Request(lot, dilemmas: true));
                if (pick.Archetype != CarArchetype.Muddy) continue;
                foundSeed = seed;
                return pick;
            }
            Assert.Fail("a x3 Muddy weight should win at least one of 30 draws");
            foundSeed = 0;
            return default;
        }

        [Test]
        public void NoRuleIsReportedWhenTheBudgetForbidsDilemmas()
        {
            var lot = Stations(true, true, StationSlotState.Blocked);
            for (var seed = 1; seed <= 20; seed++)
                Assert.IsFalse(Picker(seed).Pick(Request(lot, dilemmas: false)).HasRule);
        }

        [Test]
        public void AnUnsolvableRuleBoostsNothing()
        {
            var lot = Stations(true, true, StationSlotState.Blocked);
            lot.FreeSpots = 0;
            lot.TotalSpots = 6;
            for (var seed = 1; seed <= 30; seed++)
                Assert.IsFalse(Picker(seed).Pick(Request(lot, dilemmas: true)).HasRule);
        }

        [Test]
        public void LotTightAsksForAnEarlyReturnWhateverWins()
        {
            var lot = Stations(true, true);
            lot.FreeSpots = 1;
            lot.TotalSpots = 6;
            Assert.IsTrue(Picker(1).Pick(Request(lot, dilemmas: true)).WantsEarlyReturn);
            Assert.IsFalse(Picker(1).Pick(Request(lot, dilemmas: false)).WantsEarlyReturn);
        }

        [Test]
        public void UnderTargetFavoursMuddyAndOverTargetDoesNot()
        {
            var lot = Stations(true, true);
            Assert.Greater(CountOf(CarArchetype.Muddy, lot, ScheduleReason.UnderTarget),
                CountOf(CarArchetype.Muddy, lot, ScheduleReason.OverTarget));
        }

        private int CountOf(CarArchetype wanted, SnapshotBuilderForTests lot, ScheduleReason band)
        {
            var n = 0;
            for (var seed = 1; seed <= 300; seed++)
            {
                var request = new PickRequest(lot.Build(), ArchetypeWeights.Diner, band, false, false, BigBudget);
                if (Picker(seed).Pick(request).Archetype == wanted) n++;
            }
            return n;
        }

        [Test]
        public void AssistModeWeakensTheBoostTowardOne()
        {
            var tuning = new TuningStore(new EventBus());
            tuning.Seed(TuningKey.DirAssist, 1f);
            tuning.Seed(TuningKey.DirAssistDilemmaScale, 0.5f);
            _settings.Refresh(tuning);
            var pick = FirstMuddyPick(out _);
            Assert.AreEqual(2f, pick.Multiplier, 0.0001f, "x3 with half the strength is x2");
        }
    }
}
