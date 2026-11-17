using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure rule of the floating gag lines: which walker pops a line now. A walker qualifies when it is on screen, within the trigger distance of
    /// the valet (or his car) and past its own lockout; nothing qualifies during the global lockout that follows every pop, so at most one line
    /// is out at a time. The nearest qualifying walker wins. Never allocates after construction.
    /// </summary>
    public sealed class GagLineRule
    {
        /// <summary>Returned by <see cref="Pick"/> when nobody pops a line.</summary>
        public const int None = -1;

        private readonly float _distanceSquared;
        private readonly float _globalLockoutSeconds;
        private readonly float _walkerLockoutSeconds;
        private readonly float[] _walkerFreeAt;
        private float _globalFreeAt = float.NegativeInfinity;

        /// <param name="distance">World units within which a walker pops a line.</param>
        /// <param name="globalLockoutSeconds">Seconds after a pop before any walker may pop.</param>
        /// <param name="walkerLockoutSeconds">Seconds after a pop before the same walker may pop again.</param>
        /// <param name="walkerCapacity">Most walkers a call to <see cref="Pick"/> may pass.</param>
        public GagLineRule(float distance, float globalLockoutSeconds, float walkerLockoutSeconds, int walkerCapacity)
        {
            _distanceSquared = distance * distance;
            _globalLockoutSeconds = globalLockoutSeconds;
            _walkerLockoutSeconds = walkerLockoutSeconds;
            _walkerFreeAt = new float[walkerCapacity];
            Reset();
        }

        /// <summary>
        /// The index of the walker that pops a line at <paramref name="now"/>, or <see cref="None"/>; the pick starts both lockouts.
        /// <paramref name="count"/> walkers are described by <paramref name="positions"/> and <paramref name="active"/> (false = out of sight).
        /// </summary>
        public int Pick(float now, Vector2 player, Vector2[] positions, bool[] active, int count)
        {
            if (now < _globalFreeAt) return None;
            var best = None;
            var bestDistance = _distanceSquared;
            for (var i = 0; i < count && i < _walkerFreeAt.Length; i++)
            {
                if (!active[i] || now < _walkerFreeAt[i]) continue;
                var distance = (positions[i] - player).sqrMagnitude;
                if (distance > bestDistance) continue;
                best = i;
                bestDistance = distance;
            }
            if (best == None) return None;
            _globalFreeAt = now + _globalLockoutSeconds;
            _walkerFreeAt[best] = now + _walkerLockoutSeconds;
            return best;
        }

        /// <summary>Forgets every lockout (a new shift).</summary>
        public void Reset()
        {
            _globalFreeAt = float.NegativeInfinity;
            for (var i = 0; i < _walkerFreeAt.Length; i++) _walkerFreeAt[i] = float.NegativeInfinity;
        }
    }
}
