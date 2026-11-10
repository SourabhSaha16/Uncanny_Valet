using NUnit.Framework;
using UncannyValet.Core.Ids;
using UncannyValet.Lot;

namespace UncannyValet.Tests.Lot
{
    public class SlotTintRuleTests
    {
        private const float Min = 0.3f;
        private const float Max = 0.9f;
        private const float DoneSeconds = 2f;
        private readonly CarId _a = new CarId(1);
        private readonly CarId _b = new CarId(2);
        private SlotTintRule _wash;
        private SlotTintRule _bay;

        [SetUp]
        public void SetUp()
        {
            _wash = new SlotTintRule(Min, Max, DoneSeconds);
            _bay = new SlotTintRule(Min, Max, DoneSeconds);
        }

        [Test]
        public void EmptySlot_IsIdleAndUnlit()
        {
            Assert.AreEqual(SlotTintPhase.Idle, _wash.Phase);
            Assert.AreEqual(0f, _wash.Share);
        }

        [Test]
        public void CarThatNeedsNothing_DoesNotLightTheSlot()
        {
            _wash.Enter(_a);
            Assert.AreEqual(SlotTintPhase.Idle, _wash.Phase);
            _wash.Progress(_b, 0.2f);
            Assert.AreEqual(0f, _wash.Share);
        }

        [Test]
        public void KnownLevelBeforeArrival_LightsTheSlotOnSettle()
        {
            _wash.Progress(_a, 0f);
            _wash.Enter(_a);
            Assert.AreEqual(SlotTintPhase.Working, _wash.Phase);
            Assert.AreEqual(Min, _wash.Share, 1e-5f);
        }

        [Test]
        public void Progress_BrightensTheSlotTowardsTheMaximum()
        {
            _wash.Enter(_a);
            _wash.Progress(_a, 0.5f);
            Assert.AreEqual(Min + (Max - Min) * 0.5f, _wash.Share, 1e-5f);
            _wash.Progress(_a, 1f);
            Assert.AreEqual(Max, _wash.Share, 1e-5f);
        }

        [Test]
        public void Finished_FlashesThenFadesBackToIdle()
        {
            _wash.Enter(_a);
            _wash.Progress(_a, 0.6f);
            _wash.Finished(_a);
            Assert.AreEqual(SlotTintPhase.Done, _wash.Phase);
            Assert.AreEqual(Max, _wash.Share, 1e-5f);
            _wash.Tick(DoneSeconds * 0.5f);
            Assert.AreEqual(Max * 0.5f, _wash.Share, 1e-5f);
            _wash.Tick(DoneSeconds);
            Assert.AreEqual(SlotTintPhase.Idle, _wash.Phase);
            Assert.AreEqual(0f, _wash.Share);
        }

        [Test]
        public void LateProgress_AfterFinishing_DoesNotRelightTheSlot()
        {
            _wash.Enter(_a);
            _wash.Finished(_a);
            _wash.Progress(_a, 0.4f);
            Assert.AreEqual(SlotTintPhase.Done, _wash.Phase);
        }

        [Test]
        public void CarDrivenOutMidWash_ClearsTheTintAtOnce()
        {
            _wash.Enter(_a);
            _wash.Progress(_a, 0.4f);
            _wash.Leave(_a);
            Assert.AreEqual(SlotTintPhase.Idle, _wash.Phase);
            Assert.AreEqual(0f, _wash.Share);
        }

        [Test]
        public void CarReturningMidWash_ResumesFromItsLevel()
        {
            _wash.Enter(_a);
            _wash.Progress(_a, 0.4f);
            _wash.Leave(_a);
            _wash.Enter(_a);
            Assert.AreEqual(Min + (Max - Min) * 0.4f, _wash.Share, 1e-5f);
        }

        [Test]
        public void TicketClosed_ClearsTheTintAndForgetsTheCar()
        {
            _wash.Enter(_a);
            _wash.Progress(_a, 0.4f);
            _wash.Closed(_a);
            Assert.AreEqual(0f, _wash.Share);
            _wash.Enter(_a);
            Assert.AreEqual(SlotTintPhase.Idle, _wash.Phase);
        }

        [Test]
        public void LeavingOfAnotherCar_DoesNotTouchTheSlot()
        {
            _wash.Enter(_a);
            _wash.Progress(_a, 0.4f);
            _wash.Leave(_b);
            _wash.Closed(_b);
            Assert.AreEqual(SlotTintPhase.Working, _wash.Phase);
        }

        [Test]
        public void TwoSlots_AreIndependent()
        {
            _wash.Enter(_a);
            _wash.Progress(_a, 0.5f);
            _bay.Enter(_b);
            _bay.Progress(_b, 0.1f);
            _wash.Finished(_a);
            Assert.AreEqual(SlotTintPhase.Done, _wash.Phase);
            Assert.AreEqual(SlotTintPhase.Working, _bay.Phase);
            _bay.Leave(_b);
            Assert.AreEqual(SlotTintPhase.Done, _wash.Phase);
        }

        [Test]
        public void ChargingBay_FullCarAlreadyInside_StaysDark()
        {
            _bay.Progress(_a, 1f);
            _bay.Enter(_a);
            Assert.AreEqual(SlotTintPhase.Idle, _bay.Phase);
            Assert.AreEqual(0f, _bay.Share);
        }

        [Test]
        public void ChargingBay_CarFillsUpAndFinishes()
        {
            _bay.Progress(_a, 0.2f);
            _bay.Enter(_a);
            _bay.Progress(_a, 0.7f);
            _bay.Progress(_a, 1f);
            _bay.Finished(_a);
            Assert.AreEqual(SlotTintPhase.Done, _bay.Phase);
            _bay.Leave(_a);
            Assert.AreEqual(0f, _bay.Share);
        }
    }
}
