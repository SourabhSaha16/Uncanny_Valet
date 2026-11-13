using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>What one soak shift produced.</summary>
    internal sealed class SoakResult
    {
        public int Arrivals;
        public int Delivered;
        public int Angry;
        public int DilemmasMade;
        public int DilemmasResolved;
        public int DilemmasUnsolvable;
        public int MostOpenAtOnce;
        /// <summary>The longest stretch with no open task and no arrival while cars were still due.</summary>
        public float LongestQuietSeconds;
        /// <summary>The longest stretch with no car in the arrival queue while the idle guard was allowed to send.</summary>
        public float LongestEmptyQueueSeconds;
        /// <summary>When the first car arrived, seconds into the shift (-1 when none did).</summary>
        public float FirstArrivalSeconds = -1f;
        /// <summary>Pickup requests closer together than the spacing, outside wave peaks: all, and those where neither car was in a station.</summary>
        public int SpacingViolationsAll;
        public int SpacingViolations;
        public int PickupsRequested;
        /// <summary>Cars still waiting at the end that arrived in time to be picked up: outside a station, or in one.</summary>
        public int UnrequestedInSpot;
        public int UnrequestedInStation;
        /// <summary>Cars still waiting at the end that were parked after the last useful pickup moment.</summary>
        public int ParkedTooLate;
        /// <summary>Returns moved outside the window before the late-shift phase.</summary>
        public int MovedOutsideWindow;
        public int Moves;
    }

    /// <summary>Runs one seeded shift: the real DirectorBrain against the fake lot and the scripted bot.</summary>
    internal static class SoakShift
    {
        private const float Dt = 0.5f;
        private const float ShiftSeconds = 240f;
        private const int MaxCombo = 4;

        public static SoakResult Run(int seed)
        {
            var bus = new EventBus();
            var tuning = new TuningStore(bus);
            var clock = new FakeClock { ShiftLengthSeconds = ShiftSeconds, SecondsRemaining = ShiftSeconds };
            var sim = new SoakSim(bus, new System.Random(seed * 31 + 7));
            var settings = new DirectorSettings();
            var state = new DirectorState();
            var random = new TestRandom(seed);
            var form = new FormTracker(bus, settings.Form, clock, null, MaxCombo);
            var rules = DilemmaRules.CreateAll(settings.Rules);
            var dilemmas = new DilemmaDirector(bus, clock, new ArchetypePicker(random, rules, settings.Picker, ModifierTestKit.Catalog()),
                new DilemmaGate(settings.Gate), rules, state);
            var lot = new SoakLotView(sim);
            var profile = new SoakProfile();
            var returns = new ReturnDirector(bus, clock, tuning, settings);
            var brain = new DirectorBrain(bus, tuning, clock, lot, profile, state, settings, form,
                new ArrivalPacer(random, settings.Pacing, new ReliefGate(settings.Relief)), dilemmas, returns);
            var result = new SoakResult();
            Watch(bus, lot, sim, settings, result);
            bus.Publish(new ShiftStarted());
            Play(clock, sim, brain, settings, result);
            result.Arrivals = sim.Arrivals;
            result.Delivered = sim.Delivered;
            result.Angry = sim.AngryTimes.Count;
            SoakReturnAudit.CountSpacing(sim, profile, ShiftSeconds, settings.Returns.SpacingSeconds, Dt, result);
            var rs = settings.Returns;
            SoakReturnAudit.CountUnrequested(sim, ShiftSeconds, rs.LastLeadSeconds,
                rs.MinLeadSeconds + 2f * rs.SpacingSeconds, result);
            brain.Dispose(); returns.Dispose(); dilemmas.Dispose(); form.Dispose(); sim.Dispose();
            return result;
        }

        private static void Play(FakeClock clock, SoakSim sim, DirectorBrain brain, DirectorSettings settings, SoakResult result)
        {
            var quiet = 0f;
            var empty = 0f;
            for (var t = Dt; t <= ShiftSeconds; t += Dt)
            {
                clock.SetElapsed(t);
                sim.Now = t;
                var before = sim.Arrivals;
                sim.Step(Dt);
                brain.Tick(Dt);
                var active = sim.HasOpenTask || sim.Arrivals != before;
                quiet = active ? 0f : quiet + Dt;
                if (result.FirstArrivalSeconds < 0f && sim.Arrivals > 0) result.FirstArrivalSeconds = t;
                var guardedByTime = ShiftSeconds - t >= settings.Pacing.IdleCutoffSeconds;
                var guarded = sim.Arrivals < settings.Pacing.MaxArrivals
                    && ShiftSeconds - t >= settings.Pacing.IdleCutoffSeconds;
                empty = sim.AwaitingCount == 0 && guarded ? empty + Dt : 0f;
                if (empty > result.LongestEmptyQueueSeconds) result.LongestEmptyQueueSeconds = empty;
                var cardsDue = sim.Arrivals < settings.Pacing.ArrivalBudget && guardedByTime;
                if (cardsDue && quiet > result.LongestQuietSeconds) result.LongestQuietSeconds = quiet;
            }
        }

        // Counts dilemmas and checks each was solvable the moment it was made.
        private static void Watch(EventBus bus, SoakLotView view, SoakSim sim, DirectorSettings settings, SoakResult result)
        {
            var open = 0;
            bus.Subscribe<DilemmaCreated>(e =>
            {
                var s = view.Capture();
                var solvable = s.FreeSpots > 0 || s.SecondsToNextPickup < settings.Rules.SolvablePickupSeconds;
                if (!solvable) result.DilemmasUnsolvable++;
                result.DilemmasMade++;
                open++;
                if (open > result.MostOpenAtOnce) result.MostOpenAtOnce = open;
            });
            bus.Subscribe<DilemmaResolved>(e =>
            {
                result.DilemmasResolved++;
                open--;
            });
            bus.Subscribe<PatronReturnShifted>(e =>
            {
                result.Moves++;
                var late = ShiftSeconds - sim.Now <= settings.Returns.LateStartSeconds;
                foreach (var car in sim.Cars)
                    if (car.Id.Value == e.Car.Value && !late && SoakReturnAudit.OutsideWindow(car, settings.Returns.WindowFraction))
                        result.MovedOutsideWindow++;
            });
        }
    }
}
