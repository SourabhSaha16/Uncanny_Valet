using UnityEngine;

namespace UncannyValet.Economy
{
    /// <summary>Tunable numbers for the curse an angry patron (pickup timer ran out) costs on delivery. Issue 166.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Overtime Curse Config", fileName = "OvertimeCurseConfig")]
    public sealed class OvertimeCurseConfig : ScriptableObject
    {
        [SerializeField, Min(0.5f), Tooltip("Seconds the patron waits beyond the pickup timer for each blessing the curse takes off (rounded, at least 1 once late).")]
        private float _secondsPerPoint = OvertimeCurseRule.DefaultSecondsPerPoint;
        [SerializeField, Min(0), Tooltip("Most blessings an overtime curse can take off. 0 switches the overtime curse off.")]
        private int _cap = OvertimeCurseRule.DefaultCap;

        public float SecondsPerPoint => _secondsPerPoint;
        public int Cap => _cap;
    }
}
