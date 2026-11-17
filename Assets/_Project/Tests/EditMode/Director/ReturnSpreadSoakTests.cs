using NUnit.Framework;

namespace UncannyValet.Tests.Director
{
    /// <summary>
    /// Issue 177: patrons were still summoned in clumps on crazy nights because the pickup spacing was waived in a wave
    /// peak. Compares 200 seeded shifts with the old waived spacing and the new peak spacing: same arrival rate, fewer
    /// simultaneous summons.
    /// </summary>
    public class ReturnSpreadSoakTests
    {
        private const int Shifts = 200;
        private const float OldPeakSpacing = 0f;

        private struct Totals { public int Worst; public int Over2; public int Requested; public int Arrivals; }

        private static Totals Measure(float peakSpacing)
        {
            var t = new Totals();
            for (var seed = 1; seed <= Shifts; seed++)
            {
                var r = SoakShift.Run(seed, peakSpacing);
                if (r.MostSummonsIn5Seconds > t.Worst) t.Worst = r.MostSummonsIn5Seconds;
                if (r.MostSummonsIn5Seconds > 2) t.Over2++;
                t.Requested += r.PickupsRequested;
                t.Arrivals += r.Arrivals;
            }
            return t;
        }

        [Test]
        public void PeakSpacingSpreadsSummonsWithoutChangingTheRate()
        {
            var before = Measure(OldPeakSpacing);
            var after = Measure(-1f);
            TestContext.WriteLine("Most summons in any 5 s: before " + before.Worst + ", after " + after.Worst
                + "; shifts with more than 2 in 5 s: before " + before.Over2 + ", after " + after.Over2
                + "; pickups requested: before " + before.Requested + ", after " + after.Requested
                + "; arrivals: before " + before.Arrivals + ", after " + after.Arrivals + " (of " + Shifts + " shifts).");
            Assert.LessOrEqual(after.Worst, before.Worst);
            Assert.LessOrEqual(after.Over2 * 2, before.Over2, "peak spacing must at least halve the shifts with a clump of 3 or more");
            Assert.AreEqual(before.Requested, after.Requested, before.Requested * 0.05f, "the average pickup rate must stay about the same");
        }
    }
}
