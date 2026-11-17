namespace UncannyValet.Powers
{
    /// <summary>The payment for a power: one bucket of the shared power bar.</summary>
    public interface IPowerBucket
    {
        /// <summary>True when a bucket is open and a power is owned. Changes nothing.</summary>
        bool CanSpend { get; }

        /// <summary>Spends one bucket; false, and nothing changes, when there is none to spend.</summary>
        bool TrySpend();
    }
}
