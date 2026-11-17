using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Per-car drift timeline (issue 151): a yaw kick on the frame the drift button starts, a turn boost while it is held,
    /// and a grip that stays loose for a hold time after release before blending back. Plain maths, no allocation.
    /// </summary>
    public sealed class DriftState
    {
        private bool _wasActive;
        private float _sinceRelease = float.PositiveInfinity;
        private float _kickLeft;
        private float _kickSign;

        /// <summary>0 = normal grip, 1 = full drift grip (see <see cref="DriftModel.Step(Vector2, Vector2, float, in DriftSettings, float)"/>).</summary>
        public float GripBlend { get; private set; }
        /// <summary>Multiplier on the turn rate: the boost while drifting, 1 otherwise.</summary>
        public float TurnScale { get; private set; } = 1f;
        /// <summary>Counter-clockwise yaw rate (radians per second) of the entry kick, to add to the turn rate.</summary>
        public float KickYaw { get; private set; }

        /// <summary>Advances one step. <paramref name="speed"/> is the speed of travel, <paramref name="forward"/> the signed speed along the facing.</summary>
        public void Advance(in DriveInput input, float speed, float forward, in DriftKickSettings s, float dt)
        {
            var active = input.Drift && speed > s.MinSpeed;
            if (active && !_wasActive) StartKick(input.Steer, forward, s);
            _wasActive = active;
            _sinceRelease = active ? 0f : _sinceRelease + dt;
            GripBlend = active ? 1f : 1f - Mathf.Clamp01((_sinceRelease - s.HoldSeconds) / Mathf.Max(0.01f, s.BlendSeconds));
            TurnScale = active ? s.TurnScale : 1f;
            _kickLeft = Mathf.Max(0f, _kickLeft - dt);
            KickYaw = _kickLeft > 0f ? _kickSign * s.KickRate * _kickLeft / Mathf.Max(0.01f, s.KickSeconds) : 0f;
        }

        private void StartKick(float steer, float forward, in DriftKickSettings s)
        {
            _kickLeft = s.KickSeconds;
            _kickSign = -Mathf.Clamp(steer, -1f, 1f) * (forward < 0f ? -1f : 1f);
        }
    }
}
