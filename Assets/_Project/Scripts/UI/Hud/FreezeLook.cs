using System;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>Sizes, colours and pure maths of the freeze time look: a cold blue wash with a clear bubble round the player, and frost at the screen edge.</summary>
    public static class FreezeLook
    {
        /// <summary>Fade in and out times in real seconds.</summary>
        public const float FadeInSeconds = 0.25f;
        public const float FadeOutSeconds = 0.4f;

        /// <summary>Height of one wash row in canvas pixels (chunky rows keep the pixel look and the draw count low).</summary>
        public const int RowHeight = 2;

        /// <summary>Half size of the clear bubble round the player, in canvas pixels, and the width of its half-strength rim.</summary>
        public const int BubbleRadiusX = 40;
        public const int BubbleRadiusY = 34;
        public const int BubbleRim = 5;

        /// <summary>Frost: band thickness and count at the edge, spacing and size range of the ice spikes.</summary>
        public const int BandThickness = 3;
        public const int BandCount = 3;
        public const int SpikeSpacing = 10;
        public const int SpikeWidth = 2;
        public const int SpikeMinLength = 5;
        public const int SpikeExtraLength = 11;

        public const float WashAlpha = 0.3f;
        public const float RimFraction = 0.5f;
        public const float BandAlpha = 0.34f;
        public const float SpikeAlpha = 0.42f;

        public static readonly Color Wash = new Color32(0x3A, 0x6E, 0xE0, 255);
        public static readonly Color Frost = new Color32(0xD8, 0xF2, 0xFF, 255);

        private const uint HashMultiplier = 2654435761u;
        private const int HashShift = 13;
        private const int IndexMix = 31;
        private const int EdgeMix = 977;
        private const int HashSeed = 7;

        /// <summary>Half width of the bubble at <paramref name="rowOffset"/> pixels from its centre row, for the given radii; 0 outside it.</summary>
        public static int BubbleHalfWidth(int rowOffset, int radiusX, int radiusY)
        {
            if (radiusY <= 0 || Math.Abs(rowOffset) >= radiusY) return 0;
            var t = (float)rowOffset / radiusY;
            return (int)(radiusX * Math.Sqrt(1f - t * t));
        }

        /// <summary>A repeatable "random" spike length in canvas pixels for the spike at <paramref name="index"/> on an edge.</summary>
        public static int SpikeLength(int index, int edge)
        {
            var h = (uint)(index * IndexMix + edge * EdgeMix + HashSeed) * HashMultiplier;
            h ^= h >> HashShift;
            return SpikeMinLength + (int)(h % (uint)(SpikeExtraLength + 1));
        }
    }
}
