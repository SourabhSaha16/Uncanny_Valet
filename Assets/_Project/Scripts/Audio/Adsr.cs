using System;

namespace UncannyValet.Audio
{
    /// <summary>Linear attack/decay/sustain/release envelope. Times in seconds, sustain is a level 0..1.</summary>
    public readonly struct Adsr
    {
        private const float MinSeconds = 1e-4f;

        public readonly float Attack;
        public readonly float Decay;
        public readonly float Sustain;
        public readonly float Release;

        public Adsr(float attack, float decay, float sustain, float release)
        {
            Attack = Math.Max(attack, MinSeconds);
            Decay = Math.Max(decay, MinSeconds);
            Sustain = Math.Min(Math.Max(sustain, 0f), 1f);
            Release = Math.Max(release, MinSeconds);
        }

        /// <summary>Seconds until the sound is silent when the note is held for <paramref name="hold"/>.</summary>
        public float TotalTime(float hold) => hold + Release;

        /// <summary>Envelope level at time <paramref name="t"/> for a note held <paramref name="hold"/> seconds.</summary>
        public float Evaluate(float t, float hold)
        {
            if (t < 0f) return 0f;
            if (t >= hold) return HeldLevel(hold) * Math.Max(0f, 1f - (t - hold) / Release);
            return HeldLevel(t);
        }

        private float HeldLevel(float t)
        {
            if (t < Attack) return t / Attack;
            if (t < Attack + Decay) return 1f - (1f - Sustain) * ((t - Attack) / Decay);
            return Sustain;
        }
    }
}
