using UncannyValet.Core.Contracts;

namespace UncannyValet.Cars
{
    /// <summary>One car's current road trip: steps it through <see cref="ITripDriver"/> and releases it when done or cancelled.</summary>
    public sealed class CarTrip
    {
        private ITripDriver _driver;
        private int _id = TripId.None;

        /// <summary>True from <see cref="Begin"/> until the trip finishes, is lost or is cancelled.</summary>
        public bool Active => _id != TripId.None;

        /// <summary>Starts following trip <paramref name="id"/> of <paramref name="driver"/>.</summary>
        public void Begin(ITripDriver driver, int id)
        {
            _driver = driver;
            _id = id;
        }

        /// <summary>Advances the trip by <paramref name="dt"/> seconds and reports where it stands.</summary>
        public TripStatus Advance(float dt, out TripPose pose)
        {
            pose = default;
            if (!Active) return TripStatus.Lost;
            if (!_driver.Step(_id, dt, out pose))
            {
                _id = TripId.None;
                return TripStatus.Lost;
            }
            if (pose.Phase != TripPhase.Finished) return TripStatus.Running;
            _id = TripId.None;
            return TripStatus.Finished;
        }

        /// <summary>Ends the trip early so the router frees everything it holds. Safe when idle.</summary>
        public void Cancel()
        {
            if (Active) _driver.Cancel(_id);
            _id = TripId.None;
        }
    }
}
