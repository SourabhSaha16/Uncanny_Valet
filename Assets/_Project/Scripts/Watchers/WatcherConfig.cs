using UnityEngine;

namespace UncannyValet.Watchers
{
    /// <summary>
    /// The tunable numbers of the speech bubbles and of what makes a watcher speak: how long a bubble stays, the lockouts between
    /// bubbles and between tips, and the thresholds of the situations. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Watcher Config", fileName = "WatcherConfig")]
    public sealed class WatcherConfig : ScriptableObject
    {
        [SerializeField, Min(0.5f), Tooltip("Seconds a speech bubble stays on screen.")]
        private float _bubbleSeconds = 4f;
        [SerializeField, Min(0.5f), Tooltip("Least seconds between the starts of two bubbles of any kind; requests in between are dropped.")]
        private float _minGapSeconds = 8f;
        [SerializeField, Min(0.5f), Tooltip("Least seconds between two tips of a watcher.")]
        private float _tipGapSeconds = 15f;
        [SerializeField, Range(0.05f, 1f), Tooltip("A single hit that adds more damage than this share of a car is a big crash the watcher comments on.")]
        private float _bigCrashDamage = 0.3f;
        [SerializeField, Range(0.3f, 0.99f), Tooltip("A patron whose patience timer is past this share has nearly run out of patience (a tip situation).")]
        private float _patienceNearlyGone = 0.8f;

        public float BubbleSeconds => _bubbleSeconds;
        public float MinGapSeconds => _minGapSeconds;
        public float TipGapSeconds => _tipGapSeconds;
        public float BigCrashDamage => _bigCrashDamage;
        public float PatienceNearlyGone => _patienceNearlyGone;
    }
}
