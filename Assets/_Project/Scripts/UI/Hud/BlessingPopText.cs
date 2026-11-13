using System;

namespace UncannyValet.UI.Hud
{
    /// <summary>Pure text, look and motion curves of a blessing pop. The text is decided once when the pop starts.</summary>
    public static class BlessingPopText
    {
        /// <summary>The label of a Perfect Park or perfect drop-off pop.</summary>
        public const string PerfectLabel = "PERFECT!";

        /// <summary>The label: "+N" for a blessing (drawn with the bolt), "0" for none.</summary>
        public static string Format(int blessing) => blessing > 0 ? "+" + blessing : "0";

        /// <summary>Which look a blessing gets; a threshold of 0 turns the big look off.</summary>
        public static BlessingPopKind KindOf(int blessing, int bigBlessingAmount)
        {
            if (blessing <= 0) return BlessingPopKind.Zero;
            return bigBlessingAmount > 0 && blessing >= bigBlessingAmount ? BlessingPopKind.Big : BlessingPopKind.Normal;
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
