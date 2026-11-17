using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Works out a car hit on a pedestrian: how far and how long they are thrown (see <see cref="KnockbackCurve"/>), then throws them.</summary>
    public static class WalkerKnockback
    {
        /// <summary>
        /// Knocks <paramref name="walker"/> away from <paramref name="carPosition"/> for a hit at <paramref name="speed"/>. False for a touch
        /// slower than <paramref name="minSpeed"/> or while the person is already tumbling.
        /// </summary>
        public static bool TryApply(PedestrianWalker walker, Vector2 carPosition, float speed, float minSpeed)
        {
            if (speed < minSpeed) return false;
            Services.TryGet<ITuning>(out var tuning);
            var curve = KnockbackCurve.FromTuning(tuning);
            return walker.TryKnockback(walker.Position - carPosition, curve.Distance(speed), curve.Seconds(speed));
        }

        /// <summary>The event for a car that knocked <paramref name="walker"/> away at <paramref name="speed"/>.</summary>
        public static PedestrianHit ToEvent(PedestrianWalker walker, IDrivable car, float speed)
            => new PedestrianHit(new PedestrianId(walker.GetInstanceID()), walker.Brain.IsPatron, walker.Position, speed, car.HasDriver, car.Id);
    }
}
