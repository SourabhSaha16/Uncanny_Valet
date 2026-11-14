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
    /// <summary>Hitting a patron walker with the driven car is a curse of 5 (once per patron per 3 s); nothing else is.</summary>
    public class PedestrianCurseTests
    {
        private static PedestrianHit Hit(int patron, bool isPatron = true, bool byPlayer = true)
            => new PedestrianHit(new PedestrianId(patron), isPatron, Vector2.zero, 5f, byPlayer, new CarId(1));

        [Test]
        public void Rule_CursesOnlyTheDrivenCarHittingAPatron()
        {
            var rule = new PedestrianCurseRule();
            Assert.IsFalse(rule.TryCurse(Hit(1, isPatron: false), 0f, true, 3f), "ambient walker");
            Assert.IsFalse(rule.TryCurse(Hit(1, byPlayer: false), 0f, true, 3f), "a game-driven car");
            Assert.IsFalse(rule.TryCurse(Hit(1), 0f, false, 3f), "training lot");
            Assert.IsTrue(rule.TryCurse(Hit(1), 0f, true, 3f));
        }

        [Test]
        public void Rule_LocksOutThePatronForThreeSecondsButNotOthers()
        {
            var rule = new PedestrianCurseRule();
            Assert.IsTrue(rule.TryCurse(Hit(1), 10f, true, 3f));
            Assert.IsFalse(rule.TryCurse(Hit(1), 12.9f, true, 3f));
            Assert.IsTrue(rule.TryCurse(Hit(2), 12f, true, 3f));
            Assert.IsTrue(rule.TryCurse(Hit(1), 13f, true, 3f));
        }

        [Test]
        public void Rule_ResetClearsTheLockouts()
        {
            var rule = new PedestrianCurseRule();
            rule.TryCurse(Hit(1), 0f, true, 3f);
            rule.Reset();
            Assert.IsTrue(rule.TryCurse(Hit(1), 0.5f, true, 3f));
        }

        private sealed class World : System.IDisposable
        {
            public readonly EventBus Bus = new EventBus();
            public readonly TuningStore Tuning;
            public readonly ShiftConfig Config = ScriptableObject.CreateInstance<ShiftConfig>();
            public readonly ShiftBlessings Blessings;
            public float Now;

            public World(bool cursesOn)
            {
                Services.Reset();
                Tuning = new TuningStore(Bus);
                PedestrianHitRows.Seed(Tuning, cursesOn);
                Services.Register<ITuning>(Tuning);
                Blessings = new ShiftBlessings(Bus, Config) { Clock = () => Now };
                Bus.Publish(new ShiftStarted());
            }

            public void Earn(int amount) => Bus.Publish(new BlessingEarned(new TicketId(1), amount));

            public void Dispose()
            {
                Blessings.Dispose();
                Services.Reset();
                Object.DestroyImmediate(Config);
            }
        }

        [Test]
        public void RealBlessings_LoseFiveAndResetFavourOncePerPatronPerThreeSeconds()
        {
            using (var w = new World(true))
            {
                w.Earn(20);
                w.Earn(20);
                Assert.Greater(w.Blessings.FavourMultiplier, 1);
                var before = w.Blessings.Blessings;
                w.Bus.Publish(Hit(7));
                Assert.AreEqual(before - 5, w.Blessings.Blessings);
                Assert.AreEqual(1, w.Blessings.Curses);
                Assert.AreEqual(1, w.Blessings.FavourMultiplier);
                var after = w.Blessings.Blessings;
                w.Now = 1f;
                w.Bus.Publish(Hit(7));
                Assert.AreEqual(after, w.Blessings.Blessings);
                w.Now = 3.1f;
                w.Bus.Publish(Hit(7));
                Assert.AreEqual(after - 5, w.Blessings.Blessings);
            }
        }

        [Test]
        public void RealBlessings_NeverGoBelowZero()
        {
            using (var w = new World(true))
            {
                w.Bus.Publish(Hit(7));
                Assert.AreEqual(0, w.Blessings.Blessings);
                Assert.AreEqual(1, w.Blessings.Curses);
            }
        }

        [Test]
        public void RealBlessings_TrainingLotNeverCurses()
        {
            using (var w = new World(false))
            {
                w.Earn(40);
                var before = w.Blessings.Blessings;
                w.Bus.Publish(Hit(7));
                Assert.AreEqual(before, w.Blessings.Blessings);
                Assert.AreEqual(0, w.Blessings.Curses);
            }
        }

        [Test]
        public void RealBlessings_AmbientWalkerAndGameCarsNeverCurse()
        {
            using (var w = new World(true))
            {
                w.Earn(40);
                w.Bus.Publish(Hit(7, isPatron: false));
                w.Bus.Publish(Hit(8, byPlayer: false));
                Assert.AreEqual(0, w.Blessings.Curses);
            }
        }
    }
}
