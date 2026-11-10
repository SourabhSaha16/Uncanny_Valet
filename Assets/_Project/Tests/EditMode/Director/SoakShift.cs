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
            var dilemmas = new DilemmaDirector(bus, clock, new ArchetypePicker(random, rules, settings.Picker),
                new DilemmaGate(settings.Gate), rules, state);
            var lot = new SoakLotView(sim);
            var brain = new DirectorBrain(bus, tuning, clock, lot, new SoakProfile(), state, settings, form,
                new ArrivalPacer(random, settings.Pacing, new ReliefGate(settings.Relief)), dilemmas);
            var result = new SoakResult();
            Watch(bus, lot, settings, result);
            bus.Publish(new ShiftStarted());
            Play(clock, sim, brain, settings, result);
            result.Arrivals = sim.Arrivals;
            result.Delivered = sim.Delivered;
            result.Angry = sim.AngryTimes.Count;
            brain.Dispose(); dilemmas.Dispose(); form.Dispose(); sim.Dispose();
            return result;
        }

        private static void Play(FakeClock clock, SoakSim sim, DirectorBrain brain, DirectorSettings settings, SoakResult result)
        {
            var quiet = 0f;
            for (var t = Dt; t <= ShiftSeconds; t += Dt)
            {
                clock.SetElapsed(t);
                sim.Now = t;
                var before = sim.Arrivals;
                sim.Step(Dt);
                brain.Tick(Dt);
                var active = sim.HasOpenTask || sim.Arrivals != before;
                quiet = active ? 0f : quiet + Dt;
                var cardsDue = sim.Arrivals < settings.Pacing.ArrivalBudget;
                if (cardsDue && quiet > result.LongestQuietSeconds) result.LongestQuietSeconds = quiet;
            }
        }

        // Counts dilemmas and checks each was solvable the moment it was made.
        private static void Watch(EventBus bus, SoakLotView view, DirectorSettings settings, SoakResult result)
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
        }
    }
}
