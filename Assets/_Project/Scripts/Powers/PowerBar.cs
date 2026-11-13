using System;

namespace UncannyValet.Powers
{
    /// <summary>
    /// The shared power bar: one value from 0 to 100 (clamped, no decay), shown as three buckets. Bucket 1 opens at 33.3,
    /// bucket 2 at 66.7 and bucket 3 at 100. Using a power needs the value to be at least one bucket (33.3) and takes
    /// exactly that from the bar, so partial progress is kept.
    /// </summary>
    public sealed class PowerBar
    {
        /// <summary>The full bar.</summary>
        public const float Full = 100f;

        /// <summary>The value of one bucket: a third of the bar. Not a tunable.</summary>
        public const float BucketSize = 33.3f;

        /// <summary>Number of buckets.</summary>
        public const int BucketCount = 3;

        // Float slack so 100 - 33.3 still counts as the 66.7 mark.
        private const float Slack = 0.001f;

        private static readonly float[] Thresholds = { 33.3f, 66.7f, 100f };

        /// <summary>Current value, 0 to 100.</summary>
        public float Value { get; private set; }

        /// <summary>Buckets open (usable), 0 to 3.</summary>
        public int OpenBuckets => BucketsAt(Value);

        /// <summary>True when a power can be paid for.</summary>
        public bool CanSpend => Value + Slack >= BucketSize;

        /// <summary>Bar fill, 0 to 1.</summary>
        public float Fill01 => Value / Full;

        /// <summary>Adds to the bar (clamped to 100). Returns true when the value changed.</summary>
        public bool Add(float amount)
        {
            if (amount <= 0f || Value >= Full) return false;
            Value = Math.Min(Value + amount, Full);
            return true;
        }

        /// <summary>
        /// Spends one bucket if the bar holds one. <paramref name="bucket"/> is the bucket that closes (the last open one, 1 to 3).
        /// Returns false, and changes nothing, below 33.3.
        /// </summary>
        public bool TrySpend(out int bucket)
        {
            bucket = 0;
            if (!CanSpend) return false;
            bucket = OpenBuckets;
            Value = Math.Max(0f, Value - BucketSize);
            return true;
        }

        /// <summary>Empties the bar. Returns true when it was not already empty.</summary>
        public bool Reset()
        {
            if (Value <= 0f) return false;
            Value = 0f;
            return true;
        }

        private static int BucketsAt(float value)
        {
            var open = 0;
            while (open < BucketCount && value + Slack >= Thresholds[open]) open++;
            return open;
        }
    }
}
