using System;

namespace UncannyValet.Core
{
    /// <summary>
    /// Lets a module scale its own delta time by the world time scale (freeze time) without per-frame lookups.
    /// It resolves <see cref="Contracts.IWorldTime"/> through <see cref="Services"/> lazily and looks again once per
    /// <see cref="RefreshSeconds"/> of real time, so a contract registered after the module started (or replaced by a
    /// scene reload) is picked up. With no contract registered the scale is 1.
    /// </summary>
    public sealed class WorldTimeReader
    {
        /// <summary>Real seconds between lookups of the registered contract.</summary>
        public const float RefreshSeconds = 1f;

        private Contracts.IWorldTime _time;
        private float _sinceLookup = RefreshSeconds;

        /// <summary>The current scale: 0 = frozen, 1 = normal; never negative. Does not look anything up.</summary>
        public float Scale => _time != null ? Math.Max(0f, _time.Scale) : 1f;

        /// <summary>True while the world runs slower than normal.</summary>
        public bool IsSlowed => Scale < 1f;

        /// <summary>Returns <paramref name="realDelta"/> times the scale, and keeps the lookup fresh.</summary>
        public float Scaled(float realDelta)
        {
            _sinceLookup += realDelta;
            if (_sinceLookup >= RefreshSeconds)
            {
                _sinceLookup = 0f;
                Services.TryGet(out _time);
            }
            return realDelta * Scale;
        }
    }
}
