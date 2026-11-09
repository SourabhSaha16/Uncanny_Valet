using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;
using UncannyValet.UI;

namespace UncannyValet.Tests.Director
{
    public class DirectorOverlayTextTests
    {
        [Test]
        public void ClockFormatsMinutesAndSeconds()
        {
            Assert.AreEqual("0:00", DirectorOverlayText.Clock(0f));
            Assert.AreEqual("1:05", DirectorOverlayText.Clock(65.9f));
            Assert.AreEqual("0:00", DirectorOverlayText.Clock(-4f));
        }

        [Test]
        public void ScheduledDecisionNamesTheReasonAndTheCounter()
        {
            var d = new DirectorDecision(DecisionKind.Scheduled, 65f, ScheduleReason.UnderTarget, ReliefTrigger.Workload,
                CarArchetype.Plain, false, 6, 0.6f, 0.8f);
            var text = DirectorOverlayText.Decision(d);
            StringAssert.StartsWith("1:05 sent any (UnderTarget)", text);
            StringAssert.Contains("6 used", text);
        }

        [Test]
        public void ChosenArchetypeIsNamedAndReliefEntriesReadClearly()
        {
            var chosen = new DirectorDecision(DecisionKind.Scheduled, 10f, ScheduleReason.ReliefCar, ReliefTrigger.Workload,
                CarArchetype.Plain, true, 1, 1f, 1f);
            StringAssert.Contains("sent Plain (ReliefCar)", DirectorOverlayText.Decision(chosen));
            var started = new DirectorDecision(DecisionKind.ReliefStarted, 20f, ScheduleReason.OnTarget, ReliefTrigger.Pressure,
                CarArchetype.Plain, false, 1, 1f, 1f);
            StringAssert.Contains("RELIEF started (Pressure)", DirectorOverlayText.Decision(started));
        }

        [Test]
        public void SlotLineShowsKindStateAndCar()
        {
            var wash = new StationSlot(StationKind.Wash, StationSlotState.Blocked, new CarId(4));
            var bay = new StationSlot(StationKind.Charging, StationSlotState.Free, CarId.None);
            Assert.AreEqual("Wash: Blocked (Car#4)", DirectorOverlayText.Slot(wash, 0));
            Assert.AreEqual("Bay 2: Free", DirectorOverlayText.Slot(bay, 2));
        }

        [Test]
        public void EverySlotStateHasItsOwnColour()
        {
            var seen = new System.Collections.Generic.HashSet<UnityEngine.Color>();
            foreach (StationSlotState state in System.Enum.GetValues(typeof(StationSlotState)))
                Assert.IsTrue(seen.Add(DirectorOverlayText.SlotColor(state)), state + " repeats a colour");
        }
    }
}
