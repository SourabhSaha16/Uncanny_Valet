using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Audio
{
    public class UrgencyTests
    {
        private static CarId Car(int n) => new CarId(n);
        private static TicketId Ticket(int n) => new TicketId(n);

        [Test]
        public void TargetRisesLinearlyAndClampsAtTheMaximum()
        {
            var rule = new UrgencyRule(1f, 1.4f, 4, 1f);
            Assert.AreEqual(1f, rule.Target(0), 1e-5f);
            Assert.AreEqual(1.2f, rule.Target(2), 1e-5f);
            Assert.AreEqual(1.4f, rule.Target(4), 1e-5f);
            Assert.AreEqual(1.4f, rule.Target(9), 1e-5f);
            Assert.AreEqual(1f, rule.Target(-3), 1e-5f);
        }

        [Test]
        public void StepEasesTowardTargetWithoutOvershootAndRelaxesBack()
        {
            var rule = new UrgencyRule(1f, 1.4f, 4, 2f);
            float previous = rule.Current;
            for (int i = 0; i < 600; i++)
            {
                float now = rule.Step(4, 0.016f);
                Assert.GreaterOrEqual(now, previous);
                Assert.LessOrEqual(now, 1.4f);
                previous = now;
            }
            Assert.AreEqual(1.4f, rule.Current, 0.01f);
            for (int i = 0; i < 1200; i++) rule.Step(0, 0.016f);
            Assert.AreEqual(1f, rule.Current, 0.01f);
        }

        [Test]
        public void StepIsGradualNotInstant()
        {
            var rule = new UrgencyRule(1f, 1.4f, 4, 2f);
            Assert.Less(rule.Step(4, 0.016f), 1.01f);
        }

        [Test]
        public void ZeroSmoothingJumpsToTarget()
        {
            var rule = new UrgencyRule(1f, 1.4f, 4, 0f);
            Assert.AreEqual(1.4f, rule.Step(4, 0.016f), 1e-5f);
        }

        [Test]
        public void TrackerCountsDropOffsAndPickupsAndReleasesThem()
        {
            var bus = new EventBus();
            var tracker = new PendingCarTracker(bus);
            tracker.Subscribe();
            bus.Publish(new TicketOpened(Ticket(1), Car(1), 45f));
            bus.Publish(new TicketOpened(Ticket(2), Car(2), 45f));
            Assert.AreEqual(2, tracker.Count);
            bus.Publish(new CarParked(Car(1), 1));
            Assert.AreEqual(1, tracker.Count);
            bus.Publish(new PickupRequested(Ticket(1), Car(1)));
            Assert.AreEqual(2, tracker.Count);
            bus.Publish(new CarParked(Car(1), 2));
            Assert.AreEqual(2, tracker.Count);
            bus.Publish(new TicketClosed(Ticket(1), Car(1), 5f, 10));
            Assert.AreEqual(1, tracker.Count);
            bus.Publish(new TicketTimerExpired(Ticket(2), Car(2), TimerKind.Park));
            Assert.AreEqual(0, tracker.Count);
        }

        [Test]
        public void TrackerIgnoresPickupTimerExpiryResetsOnNightAndStopsWhenUnsubscribed()
        {
            var bus = new EventBus();
            var tracker = new PendingCarTracker(bus);
            tracker.Subscribe();
            tracker.Subscribe();
            bus.Publish(new PickupRequested(Ticket(1), Car(1)));
            bus.Publish(new TicketTimerExpired(Ticket(1), Car(1), TimerKind.Pickup));
            Assert.AreEqual(1, tracker.Count);
            bus.Publish(new ShiftEnded());
            Assert.AreEqual(0, tracker.Count);
            bus.Publish(new TicketOpened(Ticket(3), Car(3), 45f));
            bus.Publish(new ShiftStarted());
            Assert.AreEqual(0, tracker.Count);
            tracker.Unsubscribe();
            bus.Publish(new TicketOpened(Ticket(4), Car(4), 45f));
            Assert.AreEqual(0, tracker.Count);
        }
    }
}
