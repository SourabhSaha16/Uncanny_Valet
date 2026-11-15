using NUnit.Framework;
using UncannyValet.Pedestrians;

namespace UncannyValet.Tests.Pedestrians
{
    public class PedestrianBrainTests
    {
        [Test]
        public void StartsIdleAndNotDue()
        {
            var brain = new PedestrianBrain();
            Assert.AreEqual(PedestrianPhase.Idle, brain.Phase);
            Assert.IsTrue(brain.SpawnDue);
        }

        [Test]
        public void AmbientRoamsThenEntersThenComesOutAndRoamsAgain()
        {
            var brain = new PedestrianBrain();
            brain.BeginAmbient(3f, 4f);
            Assert.AreEqual(PedestrianPhase.Roaming, brain.Phase);
            brain.Tick(3f);
            Assert.IsFalse(brain.EnterDue);
            brain.Tick(1.5f);
            Assert.IsTrue(brain.EnterDue);
            brain.GoToDoor();
            Assert.AreEqual(PedestrianPhase.Arriving, brain.Phase);
            brain.ReachedRouteEnd();
            Assert.AreEqual(PedestrianPhase.Inside, brain.Phase);
            brain.Tick(2f);
            Assert.AreEqual(PedestrianPhase.Inside, brain.Phase);
            brain.Tick(1.5f);
            Assert.AreEqual(PedestrianPhase.Roaming, brain.Phase);
            Assert.IsFalse(brain.EnterDue);
        }

        [Test]
        public void PatronIsNeverDueToEnter()
        {
            var brain = new PedestrianBrain();
            brain.BeginPatron();
            Assert.IsFalse(brain.EnterDue);
        }

        [Test]
        public void PlannedVisitSetsTheNextStayAndRoam()
        {
            var brain = new PedestrianBrain();
            brain.BeginAmbient(1f, 1f);
            brain.GoToDoor();
            brain.PlanVisit(5f, 9f);
            brain.ReachedRouteEnd();
            brain.Tick(4.9f);
            Assert.AreEqual(PedestrianPhase.Inside, brain.Phase);
            brain.Tick(0.2f);
            Assert.AreEqual(PedestrianPhase.Roaming, brain.Phase);
            brain.Tick(8.9f);
            Assert.IsFalse(brain.EnterDue);
            brain.Tick(0.2f);
            Assert.IsTrue(brain.EnterDue);
        }

        [Test]
        public void SwitchedOffRoamerWaitsToAppearAgain()
        {
            var brain = new PedestrianBrain();
            brain.BeginAmbient(3f, 4f);
            brain.WaitToSpawn(2f);
            Assert.AreEqual(PedestrianPhase.Idle, brain.Phase);
            Assert.IsFalse(brain.SpawnDue);
            brain.Tick(2.5f);
            Assert.IsTrue(brain.SpawnDue);
        }

        [Test]
        public void PatronStaysInsideUntilSummonedThenWalksToTheCurbAndWaits()
        {
            var brain = new PedestrianBrain();
            brain.BeginPatron();
            Assert.AreEqual(PedestrianPhase.ToDoor, brain.Phase);
            brain.ReachedRouteEnd();
            Assert.AreEqual(PedestrianPhase.Inside, brain.Phase);
            brain.Tick(1000f);
            Assert.AreEqual(PedestrianPhase.Inside, brain.Phase);
            brain.Summon();
            Assert.AreEqual(PedestrianPhase.ToCurb, brain.Phase);
            brain.ReachedRouteEnd();
            Assert.AreEqual(PedestrianPhase.Waiting, brain.Phase);
        }

        [Test]
        public void SummonWhileStillWalkingToTheDoorIsRemembered()
        {
            var brain = new PedestrianBrain();
            brain.BeginPatron();
            brain.Summon();
            Assert.AreEqual(PedestrianPhase.ToDoor, brain.Phase);
            brain.ReachedRouteEnd();
            Assert.AreEqual(PedestrianPhase.ToCurb, brain.Phase);
        }

        [Test]
        public void PatronAlreadyInsideCanBeSummoned()
        {
            var brain = new PedestrianBrain();
            brain.BeginPatronInside();
            brain.Summon();
            Assert.AreEqual(PedestrianPhase.ToCurb, brain.Phase);
        }

        [Test]
        public void DeliveredWaitingPatronLeavesToBoardThenVanishes()
        {
            var brain = WaitingPatron();
            brain.Dismiss(true);
            Assert.AreEqual(PedestrianPhase.Leaving, brain.Phase);
            brain.ReachedRouteEnd();
            Assert.AreEqual(PedestrianPhase.Idle, brain.Phase);
        }

        [Test]
        public void PatronWithoutDeliveryDespawnsQuietly()
        {
            var brain = WaitingPatron();
            brain.Dismiss(false);
            Assert.AreEqual(PedestrianPhase.Idle, brain.Phase);
        }

        [Test]
        public void PatronNotYetAtTheCurbCannotBoard()
        {
            var brain = new PedestrianBrain();
            brain.BeginPatron();
            brain.Dismiss(true);
            Assert.AreEqual(PedestrianPhase.Idle, brain.Phase);
        }

        [Test]
        public void TimersDoNotRunForPatrons()
        {
            var brain = new PedestrianBrain();
            brain.BeginPatronInside();
            brain.Tick(100f);
            Assert.AreEqual(PedestrianPhase.Inside, brain.Phase);
            Assert.IsFalse(brain.SpawnDue);
        }

        private static PedestrianBrain WaitingPatron()
        {
            var brain = new PedestrianBrain();
            brain.BeginPatronInside();
            brain.Summon();
            brain.ReachedRouteEnd();
            return brain;
        }
    }
}
