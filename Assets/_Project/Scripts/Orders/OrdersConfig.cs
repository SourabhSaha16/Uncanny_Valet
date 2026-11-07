using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Orders
{
    /// <summary>
    /// Boss order numbers (design.md section 6). Times are game minutes. At runtime read the live values from
    /// ITuning; Bootstrap seeds them once with <see cref="SeedInto"/>. Never modified at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Orders Config", fileName = "OrdersConfig")]
    public sealed class OrdersConfig : ScriptableObject
    {
        [Tooltip("First night the boss sends orders. Every later night has them too.")]
        [SerializeField, Min(1)] private int _fromNight = 2;

        [Tooltip("Shortest wait, in game minutes, between two orders.")]
        [SerializeField, Range(1f, 90f)] private float _gapMinMinutes = 25f;

        [Tooltip("Longest wait, in game minutes, between two orders.")]
        [SerializeField, Range(1f, 120f)] private float _gapMaxMinutes = 45f;

        [Tooltip("Orders that may be open at once.")]
        [SerializeField, Range(1, 4)] private int _maxOpenOrders = 2;

        [Tooltip("Strikes, over the whole run, that lose the run.")]
        [SerializeField, Range(1, 9)] private int _maxStrikes = 3;

        public int FromNight => _fromNight;
        public float GapMinMinutes => _gapMinMinutes;
        public float GapMaxMinutes => Mathf.Max(_gapMinMinutes, _gapMaxMinutes);
        public int MaxOpenOrders => _maxOpenOrders;
        public int MaxStrikes => _maxStrikes;

        /// <summary>Gives ITuning its starting value for every Orders key (Bootstrap calls this once).</summary>
        public void SeedInto(ITuning tuning)
        {
            tuning.Seed(TuningKey.OrderGapMinMinutes, GapMinMinutes);
            tuning.Seed(TuningKey.OrderGapMaxMinutes, GapMaxMinutes);
            tuning.Seed(TuningKey.MaxOpenOrders, MaxOpenOrders);
            tuning.Seed(TuningKey.MaxStrikes, MaxStrikes);
        }
    }
}
