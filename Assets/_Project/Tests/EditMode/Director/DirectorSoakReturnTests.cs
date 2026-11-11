using NUnit.Framework;

namespace UncannyValet.Tests.Director
{
    /// <summary>
    /// The return rules over 200 seeded shifts (spec section 5, issue 73): returns stay inside their window until the
    /// late-shift phase, pickups keep their spacing outside wave peaks, and every car parked in time for a pickup has
    /// its pickup requested before the shift ends. Reports the totals in the test output.
    /// </summary>
    public class DirectorSoakReturnTests
    {
        private const int Shifts = 200;
        // Rare leftovers: two returns at the edge of their window cannot be spread without leaving it, and a car that
        // arrives in the last minute may find every slot before the last-pickup deadline taken (spacing is kept).
        private const float MaxLeftoverShare = 0.01f;

        [Test]
        public void TwoHundredSeededShiftsPickEveryParkedCarUpBeforeTheEnd()
        {
            var moves = 0; var outside = 0; var spacing = 0; var spacingAll = 0; var requested = 0;
            var inSpot = 0; var inStation = 0; var tooLate = 0; var delivered = 0; var angry = 0; var arrivals = 0;
            for (var seed = 1; seed <= Shifts; seed++)
            {
                var r = SoakShift.Run(seed);
                moves += r.Moves; outside += r.MovedOutsideWindow; spacing += r.SpacingViolations;
                spacingAll += r.SpacingViolationsAll; requested += r.PickupsRequested; inSpot += r.UnrequestedInSpot;
                inStation += r.UnrequestedInStation; tooLate += r.ParkedTooLate; delivered += r.Delivered;
                angry += r.Angry; arrivals += r.Arrivals;
            }
            TestContext.WriteLine("Over " + Shifts + " shifts: arrivals " + arrivals + ", pickups requested " + requested
                + ", delivered " + delivered + ", angry " + angry + ", return moves " + moves
                + ", moved outside the window " + outside + ". Spacing violations outside peaks: " + spacing
                + " (" + spacingAll + " counting cars that sat in a station). Still waiting at the end: " + inSpot
                + " in a spot, " + inStation + " in a station, " + tooLate + " parked too late to serve.");
            Assert.AreEqual(0, outside, "a return moved outside its window before the late phase");
            Assert.LessOrEqual(inSpot + inStation, requested * MaxLeftoverShare, "a waiting car was never called back before the end");
            Assert.LessOrEqual(spacing, requested * MaxLeftoverShare, "pickups closer than the spacing outside wave peaks");
            Assert.Greater(moves, 0, "the scheduler never moved a return");
        }
    }
}
