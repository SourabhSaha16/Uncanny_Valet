using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure text, look and motion curves of a tip pop. The text is decided once when the pop starts.</summary>
    public static class TipPopText
    {
        /// <summary>The label: "+$N" for a tip, "$0" for none.</summary>
        public static string Format(int tip) => tip > 0 ? "+$" + tip : "$0";

        /// <summary>Which look a tip gets; a threshold of 0 turns the big look off.</summary>
        public static TipPopKind KindOf(int tip, int bigTipDollars)
        {
            if (tip <= 0) return TipPopKind.Zero;
            return bigTipDollars > 0 && tip >= bigTipDollars ? TipPopKind.Big : TipPopKind.Normal;
        }

        /// <summary>Opacity 0..1 at an age: opaque through the hold fraction, then a linear fade to nothing.</summary>
        public static float Alpha(float age, float duration, float holdFraction)
        {
            var t = Math.Max(0f, Math.Min(1f, age / duration));
            return t <= holdFraction ? 1f : 1f - (t - holdFraction) / (1f - holdFraction);
        }

        /// <summary>Eased rise 0..1 (fast start, slow end) at an age.</summary>
        public static float Rise(float age, float duration)
        {
            var t = Math.Max(0f, Math.Min(1f, age / duration));
            return 1f - (1f - t) * (1f - t);
        }
    }
}
