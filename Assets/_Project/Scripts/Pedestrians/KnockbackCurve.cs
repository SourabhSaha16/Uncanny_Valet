using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure rule: how far a pedestrian is thrown and how long they tumble for a hit speed. Below the near speed the near
    /// values apply; between the near and far speeds both rise linearly; above the far speed they stay at the far values.
    /// </summary>
    public readonly struct KnockbackCurve
    {
        private readonly float _nearSpeed;
        private readonly float _farSpeed;
        private readonly float _nearDistance;
        private readonly float _farDistance;
        private readonly float _nearSeconds;
        private readonly float _farSeconds;

        public KnockbackCurve(float nearSpeed, float farSpeed, float nearDistance, float farDistance, float nearSeconds, float farSeconds)
        {
            _nearSpeed = nearSpeed; _farSpeed = Mathf.Max(farSpeed, nearSpeed + 0.01f);
            _nearDistance = nearDistance; _farDistance = farDistance;
            _nearSeconds = nearSeconds; _farSeconds = farSeconds;
        }

        /// <summary>The curve from the live tuning; keys that were never seeded fall back to the defaults.</summary>
        public static KnockbackCurve FromTuning(ITuning t) => new KnockbackCurve(
            Read(t, TuningKey.PedestrianKnockbackNearSpeed, PedestrianHitRows.NearSpeed),
            Read(t, TuningKey.PedestrianKnockbackFarSpeed, PedestrianHitRows.FarSpeed),
            Read(t, TuningKey.PedestrianKnockbackNear, PedestrianHitRows.NearDistance),
            Read(t, TuningKey.PedestrianKnockbackFar, PedestrianHitRows.FarDistance),
            Read(t, TuningKey.PedestrianTumbleNear, PedestrianHitRows.NearTumble),
            Read(t, TuningKey.PedestrianTumbleFar, PedestrianHitRows.FarTumble));

        /// <summary>Distance the person is thrown, world units.</summary>
        public float Distance(float speed) => Mathf.Lerp(_nearDistance, _farDistance, Share(speed));

        /// <summary>Seconds the person tumbles.</summary>
        public float Seconds(float speed) => Mathf.Lerp(_nearSeconds, _farSeconds, Share(speed));

        private float Share(float speed) => Mathf.Clamp01((speed - _nearSpeed) / (_farSpeed - _nearSpeed));

        private static float Read(ITuning t, TuningKey key, float fallback) => t != null && t.TryGet(key, out var v) ? v : fallback;
    }
}
