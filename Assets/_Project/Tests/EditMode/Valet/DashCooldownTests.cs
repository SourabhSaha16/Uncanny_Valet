using NUnit.Framework;
using UncannyValet.Valet;

namespace UncannyValet.Tests.Valet
{
    public class DashCooldownTests
    {
        private const float Cooldown = 1.5f;

        [Test]
        public void AFreshPressOnFootStartsADash()
        {
            var rule = new DashCooldown();
            Assert.IsTrue(rule.TryStart(true, false));
            Assert.IsTrue(rule.IsDashing);
        }

        [Test]
        public void NothingStartsWithoutAPress_OrWhileTheButtonIsOnlyHeld()
        {
            var rule = new DashCooldown();
            Assert.IsFalse(rule.TryStart(false, false));
            Assert.IsTrue(rule.TryStart(true, false));
            rule.Finish(0f);
            Assert.IsFalse(rule.TryStart(true, false), "held, not pressed again");
            Assert.IsFalse(rule.TryStart(false, false));
            Assert.IsTrue(rule.TryStart(true, false), "released and pressed again");
        }

        [Test]
        public void NoDashWhileDriving_AndAButtonHeldBeforeLeavingTheCarDoesNotFire()
        {
            var rule = new DashCooldown();
            Assert.IsFalse(rule.TryStart(true, true));
            Assert.IsFalse(rule.TryStart(true, false), "the press happened in the car");
            Assert.IsFalse(rule.IsDashing);
        }

        [Test]
        public void TheCooldownBlocksASecondDashUntilItHasRun()
        {
            var rule = new DashCooldown();
            Assert.IsTrue(rule.TryStart(true, false));
            Assert.IsFalse(rule.TryStart(false, false));
            Assert.IsFalse(rule.TryStart(true, false), "still dashing");
            rule.Finish(Cooldown);
            Assert.AreEqual(Cooldown, rule.Remaining, 0.0001f);
            rule.TryStart(false, false);
            Assert.IsFalse(rule.TryStart(true, false), "cooling down");
            rule.Tick(1.4f);
            rule.TryStart(false, false);
            Assert.IsFalse(rule.TryStart(true, false), "0.1 s left");
            rule.Tick(0.11f);
            Assert.AreEqual(0f, rule.Remaining, 0.0001f);
            rule.TryStart(false, false);
            Assert.IsTrue(rule.TryStart(true, false));
        }

        [Test]
        public void ResetClearsTheCooldownAndTheDash()
        {
            var rule = new DashCooldown();
            rule.TryStart(true, false);
            rule.Finish(Cooldown);
            rule.Reset();
            Assert.AreEqual(0f, rule.Remaining);
            Assert.IsFalse(rule.IsDashing);
            rule.TryStart(false, false);
            Assert.IsTrue(rule.TryStart(true, false));
        }

        [Test]
        public void TickNeverGoesBelowZero()
        {
            var rule = new DashCooldown();
            rule.Tick(5f);
            Assert.AreEqual(0f, rule.Remaining);
        }
    }
}
