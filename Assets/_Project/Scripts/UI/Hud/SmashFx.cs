using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>What a Park Smash particle looks like.</summary>
    public enum SmashParticleKind { Sparkle, Coin, Confetti }

    /// <summary>
    /// Pure state of the Park Smash look: the flash, the shockwave ring, the SMASH! pop and a fixed pool of particles
    /// (sparkle puffs, coins and confetti). Everything is a function of the age, so nothing is updated or allocated per frame.
    /// </summary>
    public sealed class SmashFx
    {
        /// <summary>One flying piece. World position is where it started; <see cref="PositionAt"/> moves it.</summary>
        public struct Particle
        {
            public Vector2 Start;
            public Vector2 Velocity;
            public float StartedAt;
            public float Life;
            public SmashParticleKind Kind;
            public int Shade;
            public bool Active;
        }

        private const uint SeedStart = 20261111u;
        private const uint Multiplier = 1664525u;
        private const uint Increment = 1013904223u;
        private const float TwoPi = 6.2831853f;
        private const int ShiftToUnit = 8;
        private const float UnitScale = 1f / (1 << 24);

        private readonly Particle[] _particles = new Particle[SmashLook.MaxParticles];
        private uint _seed = SeedStart;
        private float _flashAt = float.NegativeInfinity;
        private float _ringAt = float.NegativeInfinity;
        private float _popAt = float.NegativeInfinity;

        /// <summary>Where the shockwave started, in world units.</summary>
        public Vector2 RingOrigin { get; private set; }

        /// <summary>Whether the ring has a world origin (otherwise it starts at the screen centre).</summary>
        public bool RingHasOrigin { get; private set; }

        /// <summary>Text of the pop, for example "SMASH! +$120".</summary>
        public string PopText { get; private set; } = string.Empty;

        public int Capacity => _particles.Length;

        public Particle this[int index] => _particles[index];

        public float FlashAge(float now) => now - _flashAt;
        public float RingAge(float now) => now - _ringAt;
        public float PopAge(float now) => now - _popAt;

        /// <summary>The smash fires: flash and ring start.</summary>
        public void Begin(bool hasOrigin, Vector2 origin, float now)
        {
            _flashAt = now;
            _ringAt = now;
            RingHasOrigin = hasOrigin;
            RingOrigin = origin;
        }

        /// <summary>Sparkles fly out of a point where a car parked.</summary>
        public void Puff(Vector2 world, float now)
        {
            for (var i = 0; i < SmashLook.SparklesPerPuff; i++)
            {
                var angle = Next() * TwoPi;
                var speed = SmashLook.SparkleSpeed * (0.4f + 0.6f * Next());
                var velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                Spawn(world, velocity, now, SmashLook.SparkleLife, SmashParticleKind.Sparkle, 0);
            }
        }

        /// <summary>Coins and confetti are thrown up from a point where a patron was paid.</summary>
        public void Burst(Vector2 world, float now)
        {
            for (var i = 0; i < SmashLook.CoinsPerBurst; i++)
                Spawn(world, Thrown(), now, SmashLook.FallLife, SmashParticleKind.Coin, 0);
            for (var i = 0; i < SmashLook.ConfettiPerBurst; i++)
                Spawn(world, Thrown(), now, SmashLook.FallLife, SmashParticleKind.Confetti, i % SmashLook.ConfettiColors.Length);
        }

        /// <summary>Shows the SMASH! pop with its text.</summary>
        public void ShowPop(string text, float now)
        {
            PopText = text;
            _popAt = now;
        }

        /// <summary>World position of a particle <paramref name="age"/> seconds after it started.</summary>
        public static Vector2 PositionAt(in Particle p, float age)
        {
            var pos = p.Start + p.Velocity * age;
            if (p.Kind != SmashParticleKind.Sparkle) pos.y -= 0.5f * SmashLook.Gravity * age * age;
            return pos;
        }

        /// <summary>Retires particles that have lived out their life.</summary>
        public void Expire(float now)
        {
            for (var i = 0; i < _particles.Length; i++)
                if (_particles[i].Active && now - _particles[i].StartedAt >= _particles[i].Life) _particles[i].Active = false;
        }

        /// <summary>Drops everything at once (a new shift).</summary>
        public void Clear()
        {
            for (var i = 0; i < _particles.Length; i++) _particles[i].Active = false;
            _flashAt = _ringAt = _popAt = float.NegativeInfinity;
        }

        private Vector2 Thrown()
        {
            var sideways = (Next() * 2f - 1f) * SmashLook.BurstSpeed;
            var lift = SmashLook.BurstLift * (1f + Next());
            return new Vector2(sideways, lift);
        }

        private void Spawn(Vector2 world, Vector2 velocity, float now, float life, SmashParticleKind kind, int shade)
        {
            _particles[FreeOrOldest()] = new Particle
            {
                Start = world, Velocity = velocity, StartedAt = now, Life = life, Kind = kind, Shade = shade, Active = true
            };
        }

        private int FreeOrOldest()
        {
            var oldest = 0;
            for (var i = 0; i < _particles.Length; i++)
            {
                if (!_particles[i].Active) return i;
                if (_particles[i].StartedAt < _particles[oldest].StartedAt) oldest = i;
            }
            return oldest;
        }

        private float Next()
        {
            _seed = _seed * Multiplier + Increment;
            return (_seed >> ShiftToUnit) * UnitScale;
        }
    }
}
