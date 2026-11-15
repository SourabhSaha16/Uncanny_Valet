using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Economy;
using UnityEngine;

namespace UncannyValet.Tests.Economy
{
    /// <summary>Hitting any walker with the driven car is a slight curse of 2 on every lot; a patron on a curse lot gets only the full 5.</summary>
    public class SlightCurseTests
    {
        private static PedestrianHit Hit(int id, bool isPatron = false, bool byPlayer = true)
            => new PedestrianHit(new PedestrianId(id), isPatron, Vector2.zero, 5f, byPlayer, new CarId(1));

        [Test]
        public void Rule_CursesAnyWalkerOnlyForTheDrivenCar()
        {
            var rule = new SlightCurseRule();
            Assert.IsFalse(rule.TryCurse(Hit(1, byPlayer: false), 0f, 3f));
            Assert.IsTrue(rule.TryCurse(Hit(1), 0f, 3f));
            Assert.IsTrue(rule.TryCurse(Hit(2, isPatron: true), 0f, 3f));
        }

        [Test]
        public void Rule_LocksOutTheSameWalkerOnly_AndResetClears()
        {
            var rule = new SlightCurseRule();
            Assert.IsTrue(rule.TryCurse(Hit(1), 10f, 3f));
            Assert.IsFalse(rule.TryCurse(Hit(1), 12.9f, 3f));
            Assert.IsTrue(rule.TryCurse(Hit(2), 12f, 3f));
            Assert.IsTrue(rule.TryCurse(Hit(1), 13f, 3f));
            rule.Reset();
            Assert.IsTrue(rule.TryCurse(Hit(1), 13.5f, 3f));
        }

        [Test]
        public void Covers_IsTheFullCurseCase()
        {
            Assert.IsTrue(PedestrianCurseRule.Covers(Hit(1, isPatron: true), true));
            Assert.IsFalse(PedestrianCurseRule.Covers(Hit(1, isPatron: true), false));
            Assert.IsFalse(PedestrianCurseRule.Covers(Hit(1), true));
        }

        private sealed class World : System.IDisposable
        {
            public readonly EventBus Bus = new EventBus();
            public readonly ShiftConfig Config = ScriptableObject.CreateInstance<ShiftConfig>();
            public readonly ShiftBlessings Blessings;
            public float Now;

            public World(bool cursesOn)
            {
                Services.Reset();
                var tuning = new TuningStore(Bus);
                PedestrianHitRows.Seed(tuning, cursesOn);
                Services.Register<ITuning>(tuning);
                Blessings = new ShiftBlessings(Bus, Config) { Clock = () => Now };
                Bus.Publish(new ShiftStarted());
                Bus.Publish(new BlessingEarned(new TicketId(1), 40));
            }

            public void Dispose()
            {
                Blessings.Dispose();
                Services.Reset();
                Object.DestroyImmediate(Config);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AmbientHit_CursesSlightlyOnEveryLot(bool cursesOn)
        {
            using (var w = new World(cursesOn))
            {
                w.Bus.Publish(Hit(7));
                Assert.AreEqual(38, w.Blessings.Blessings);
                Assert.AreEqual(1, w.Blessings.Curses);
                Assert.AreEqual(1, w.Blessings.FavourMultiplier);
            }
        }

        [Test]
        public void PatronHit_OnCurseLot_IsOnlyTheFullCurse_EvenRepeatedInLockout()
        {
            using (var w = new World(true))
            {
                w.Bus.Publish(Hit(7, isPatron: true));
                Assert.AreEqual(35, w.Blessings.Blessings);
                Assert.AreEqual(1, w.Blessings.Curses);
                w.Now = 1f;
                w.Bus.Publish(Hit(7, isPatron: true));
                Assert.AreEqual(35, w.Blessings.Blessings, "no slight curse stacked during the full lockout");
                Assert.AreEqual(1, w.Blessings.Curses);
            }
        }

        [Test]
        public void PatronHit_OnTrainingLot_IsSlight()
        {
            using (var w = new World(false))
            {
                w.Bus.Publish(Hit(7, isPatron: true));
                Assert.AreEqual(38, w.Blessings.Blessings);
            }
        }

        [Test]
        public void Regression_OneBumpDoesNotStack_AndGameCarsNeverCurse()
        {
            using (var w = new World(false))
            {
                w.Bus.Publish(Hit(7));
                w.Now = 1f;
                w.Bus.Publish(Hit(7));
                Assert.AreEqual(38, w.Blessings.Blessings);
                w.Bus.Publish(Hit(8, byPlayer: false));
                Assert.AreEqual(1, w.Blessings.Curses);
                w.Now = 3.1f;
                w.Bus.Publish(Hit(7));
                Assert.AreEqual(36, w.Blessings.Blessings);
            }
        }
    }
}
