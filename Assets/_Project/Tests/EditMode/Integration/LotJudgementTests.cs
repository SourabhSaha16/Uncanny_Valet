using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Integration
{
    /// <summary>A whole handback and shift end through the real registry: what the perfect events pay and how the lot is judged.</summary>
    public class LotJudgementTests
    {
        private ShiftHarness _h;

        [SetUp] public void SetUp() { _h = new ShiftHarness(); }
        [TearDown] public void TearDown() { _h.Dispose(); }

        [Test]
        public void PerfectParkAndPerfectDropOff_AddNoBlessings()
        {
            _h.StartShift();
            var t = _h.Spawn(new PatronRequest(5f, 20, false));
            _h.Park(t, 1);
            _h.Bus.Publish(new PerfectPark(t.CarId, t.TicketId));
            _h.Minutes(5);
            _h.LeaveSpot(t, 1);
            _h.Bus.Publish(new PerfectDropOff(t.CarId, t.TicketId));
            _h.AtCurb(t);

            Assert.AreEqual(22, _h.Log.Last<BlessingEarned>().Amount, "same as a handback without the perfect events");
            Assert.AreEqual(22, _h.Blessings.Blessings);
        }

        [TestCase(22, true)]
        [TestCase(23, false)]
        public void ShiftEnd_ClearsOrFailsTheLotAgainstItsThreshold(int threshold, bool cleared)
        {
            _h.Goal = new FakeGoal(threshold);
            _h.StartShift();
            var t = _h.Spawn(new PatronRequest(5f, 20, false));
            _h.Park(t, 1);
            _h.Minutes(5);
            _h.LeaveSpot(t, 1);
            _h.AtCurb(t);
            _h.Minutes(1000);

            Assert.AreEqual(cleared, _h.Log.Last<ShiftResultReady>().Result.Cleared);
            Assert.AreEqual(cleared ? 1 : 0, _h.Log.Count<LotCleared>());
            Assert.AreEqual(cleared ? 0 : 1, _h.Log.Count<LotFailed>());
            Assert.Less(_h.Log.IndexOf<ShiftEnded>(), _h.Log.IndexOf<ShiftResultReady>());
        }

        private sealed class FakeGoal : ILotGoal
        {
            public FakeGoal(int threshold) { BlessingThreshold = threshold; }
            public LotId Lot { get; } = new LotId(World.Midgard, LotKind.Training);
            public int BlessingThreshold { get; }
        }
    }
}
