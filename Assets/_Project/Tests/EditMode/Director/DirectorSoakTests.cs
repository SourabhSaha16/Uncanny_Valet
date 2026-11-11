using NUnit.Framework;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>
    /// Plays seeded shifts through the real DirectorBrain with a scripted bot and a lightweight fake lot, and checks
    /// the guardrails of the spec: bounded arrivals, solvable dilemmas, no dead air. Fast: no scene, no physics.
    /// </summary>
    public class DirectorSoakTests
    {
        private const int Shifts = 200;
        private const int Budget = 14;
        private const int Slack = 2;
        private const float LongestQuietAllowed = 20f;
        // The idle limit (2 s) plus the spawn travel margin: one director tick (0.5 s) to notice and one fake-lot step.
        private static readonly float IdleLimit = new PacingSettings().MaxEmptyQueueSeconds;
        private const float TickMargin = 1f;

        [Test]
        public void TwoHundredSeededShiftsStayWithinTheGuardrails()
        {
            var dilemmas = 0;
            var resolved = 0;
            var longestEmpty = 0f;
            for (var seed = 1; seed <= Shifts; seed++)
            {
                var r = SoakShift.Run(seed);
                Assert.LessOrEqual(r.Arrivals, Budget + Slack, "arrivals, seed " + seed);
                Assert.Greater(r.Arrivals, 0, "the director sent nobody, seed " + seed);
                Assert.AreEqual(0, r.DilemmasUnsolvable, "a dilemma was made with no way out, seed " + seed);
                Assert.LessOrEqual(r.MostOpenAtOnce, 1, "two dilemmas open at once, seed " + seed);
                Assert.LessOrEqual(r.LongestQuietSeconds, LongestQuietAllowed, "dead air, seed " + seed);
                Assert.LessOrEqual(r.LongestEmptyQueueSeconds, IdleLimit + TickMargin, "empty queue too long, seed " + seed);
                Assert.That(r.FirstArrivalSeconds, Is.InRange(0f, IdleLimit + TickMargin), "first car late, seed " + seed);
                longestEmpty = System.Math.Max(longestEmpty, r.LongestEmptyQueueSeconds);
                dilemmas += r.DilemmasMade;
                resolved += r.DilemmasResolved;
            }
            TestContext.WriteLine("Longest empty-queue gap over " + Shifts + " shifts: " + longestEmpty + " s");
            Assert.Greater(dilemmas, 0, "over 200 shifts the director should make some dilemmas");
            Assert.Greater(resolved, 0, "and the bot should resolve some of them");
        }

        [Test]
        public void AShiftReplaysIdenticallyFromTheSameSeed()
        {
            var a = SoakShift.Run(42);
            var b = SoakShift.Run(42);
            Assert.AreEqual(a.Arrivals, b.Arrivals);
            Assert.AreEqual(a.Delivered, b.Delivered);
            Assert.AreEqual(a.DilemmasMade, b.DilemmasMade);
            Assert.AreEqual(a.LongestQuietSeconds, b.LongestQuietSeconds, 0.0001f);
        }
    }
}
