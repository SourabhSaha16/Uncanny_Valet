using System;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>The numbers and colours of the Park Smash look: white flash, shockwave ring, sparkles, coins, confetti and the SMASH! pop.</summary>
    public static class SmashLook
    {
        /// <summary>Seconds the white screen flash takes to fade out.</summary>
        public const float FlashSeconds = 0.35f;

        /// <summary>Strongest flash opacity (the first frame).</summary>
        public const float FlashPeakAlpha = 0.85f;

        /// <summary>Seconds the shockwave ring takes to cross the lot.</summary>
        public const float RingSeconds = 0.7f;

        /// <summary>Ring radius, in canvas pixels, when it fades out (wider than the screen).</summary>
        public const int RingMaxRadius = 330;

        /// <summary>Thickness of the ring, in canvas pixels.</summary>
        public const int RingThickness = 8;

        /// <summary>Canvas rows drawn per fill call when the ring is painted.</summary>
        public const int RingRowHeight = 2;

        /// <summary>Seconds the SMASH! pop stays up.</summary>
        public const float PopSeconds = 1.8f;

        /// <summary>Share of the pop's life it stays fully opaque before fading.</summary>
        public const float PopHoldShare = 0.6f;

        /// <summary>Canvas pixels the pop rises over its life.</summary>
        public const int PopRisePixels = 14;

        /// <summary>Font scale of the pop.</summary>
        public const int PopScale = 3;

        /// <summary>Canvas row of the top of the pop at the start (a little above the middle).</summary>
        public const int PopTop = 96;

        /// <summary>Most particles alive at once; a new one replaces the oldest.</summary>
        public const int MaxParticles = 160;

        /// <summary>Sparkles in a puff where a car parks.</summary>
        public const int SparklesPerPuff = 6;

        /// <summary>Coins in a burst where a patron is paid.</summary>
        public const int CoinsPerBurst = 6;

        /// <summary>Confetti pieces in a burst where a patron is paid.</summary>
        public const int ConfettiPerBurst = 8;

        /// <summary>Seconds a sparkle lives.</summary>
        public const float SparkleLife = 0.5f;

        /// <summary>Seconds a coin or a confetti piece lives.</summary>
        public const float FallLife = 1.1f;

        /// <summary>World units per second a sparkle flies outward (at most).</summary>
        public const float SparkleSpeed = 2.2f;

        /// <summary>World units per second a coin or confetti piece is thrown (at most, sideways).</summary>
        public const float BurstSpeed = 2.6f;

        /// <summary>World units per second a coin or confetti piece is thrown upward (at least).</summary>
        public const float BurstLift = 2.2f;

        /// <summary>Downward pull on coins and confetti, in world units per second squared.</summary>
        public const float Gravity = 7f;

        /// <summary>Side of a coin or a confetti piece, in canvas pixels.</summary>
        public const int PieceSize = 3;

        /// <summary>Length of one arm of a sparkle cross, in canvas pixels.</summary>
        public const int SparkleArm = 3;

        public static readonly Color FlashColor = Color.white;
        public static readonly Color RingColor = new Color(1f, 0.95f, 0.7f, 1f);
        public static readonly Color PopColor = PixelPalette.Gold;

        /// <summary>Confetti colours; pieces pick one by their shade index.</summary>
        public static readonly Color[] ConfettiColors =
        {
            PixelPalette.Cyan, PixelPalette.OrangeRed, PixelPalette.Green, PixelPalette.White,
            new Color32(0xFF, 0x4F, 0xA8, 255),
        };

        /// <summary>Opacity of the flash <paramref name="age"/> seconds after it started; 0 once it is over or before it starts.</summary>
        public static float FlashAlpha(float age)
        {
            if (age < 0f || age >= FlashSeconds) return 0f;
            return FlashPeakAlpha * (1f - age / FlashSeconds);
        }

        /// <summary>Radius of the ring's outer edge in canvas pixels, 0 at the start to <see cref="RingMaxRadius"/> at the end; -1 when it is not on.</summary>
        public static int RingRadius(float age)
        {
            if (age < 0f || age >= RingSeconds) return -1;
            return (int)(RingMaxRadius * (age / RingSeconds));
        }

        /// <summary>Opacity of the ring, fading out as it spreads.</summary>
        public static float RingAlpha(float age) => age < 0f || age >= RingSeconds ? 0f : 1f - age / RingSeconds;

        /// <summary>Half the width of a circle of the given radius at a vertical offset from its centre; 0 outside the circle.</summary>
        public static int HalfWidth(int radius, int offset)
        {
            var squared = radius * radius - offset * offset;
            return squared <= 0 ? 0 : (int)Math.Sqrt(squared);
        }

        /// <summary>Opacity of the pop: opaque for the hold share of its life, then fading to 0.</summary>
        public static float PopAlpha(float age)
        {
            if (age < 0f || age >= PopSeconds) return 0f;
            var hold = PopSeconds * PopHoldShare;
            return age <= hold ? 1f : 1f - (age - hold) / (PopSeconds - hold);
        }

        /// <summary>Pixels the pop has risen after <paramref name="age"/> seconds.</summary>
        public static int PopRise(float age) => age <= 0f ? 0 : (int)(PopRisePixels * Math.Min(1f, age / PopSeconds));
    }
}
