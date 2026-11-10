using NUnit.Framework;
using UncannyValet.Cars;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Tests.Cars
{
    public class CarTripTests
    {
        private sealed class ScriptedDriver : ITripDriver
        {
            public TripPhase Phase = TripPhase.OnLane;
            public bool Known = true;
            public int Cancelled;

            public bool Step(int tripId, float dt, out TripPose pose)
            {
                pose = new TripPose(Vector2.right, 90f, 1f, Phase);
                return Known;
            }

            public void Cancel(int tripId) => Cancelled++;
        }

        [Test]
        public void IsIdleUntilBegun()
        {
            var trip = new CarTrip();
            Assert.IsFalse(trip.Active);
            Assert.AreEqual(TripStatus.Lost, trip.Advance(0.1f, out _));
        }

        [Test]
        public void RunsWhileThePoseIsNotFinished()
        {
            var trip = new CarTrip();
            trip.Begin(new ScriptedDriver(), 3);
            Assert.AreEqual(TripStatus.Running, trip.Advance(0.1f, out var pose));
            Assert.AreEqual(Vector2.right, pose.Position);
            Assert.IsTrue(trip.Active);
        }

        [Test]
        public void FinishedPoseEndsTheTripWithoutCancelling()
        {
            var driver = new ScriptedDriver { Phase = TripPhase.Finished };
            var trip = new CarTrip();
            trip.Begin(driver, 3);
            Assert.AreEqual(TripStatus.Finished, trip.Advance(0.1f, out _));
            Assert.IsFalse(trip.Active);
            trip.Cancel();
            Assert.AreEqual(0, driver.Cancelled);
        }

        [Test]
        public void UnknownTripIsLost()
        {
            var trip = new CarTrip();
            trip.Begin(new ScriptedDriver { Known = false }, 3);
            Assert.AreEqual(TripStatus.Lost, trip.Advance(0.1f, out _));
            Assert.IsFalse(trip.Active);
        }

        [Test]
        public void CancelFreesTheTripOnce()
        {
            var driver = new ScriptedDriver();
            var trip = new CarTrip();
            trip.Begin(driver, 3);
            trip.Cancel();
            trip.Cancel();
            Assert.AreEqual(1, driver.Cancelled);
            Assert.IsFalse(trip.Active);
        }

        [Test]
        public void TurnTowardLimitsTheTurnRate()
        {
            Assert.AreEqual(30f, CarAutoDriveMath.TurnToward(0f, 90f, 300f, 0.1f), 0.001f);
            Assert.AreEqual(90f, CarAutoDriveMath.TurnToward(80f, 90f, 300f, 0.1f), 0.001f);
            Assert.AreEqual(-10f, CarAutoDriveMath.TurnToward(10f, -90f, 200f, 0.1f), 0.001f, "takes the short way round");
        }
    }
}
