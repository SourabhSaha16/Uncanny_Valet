using UncannyValet.Core.Contracts;

namespace UncannyValet.Traffic
{
    /// <summary>One running patron trip (entry or exit). Pooled: <see cref="Id"/> is <see cref="TripId.None"/> while idle.</summary>
    public interface ITrip
    {
        int Id { get; }

        /// <summary>Advances the trip. The pose with <see cref="TripPhase.Finished"/> is the last; the trip then goes idle.</summary>
        void Step(float dt, out TripPose pose);

        /// <summary>Stops the trip and releases its lane registration.</summary>
        void Cancel();
    }
}
