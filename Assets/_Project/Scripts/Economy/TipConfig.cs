using UnityEngine;

namespace UncannyValet.Economy
{
    /// <summary>Tunable tip numbers (design.md section 5). All values in dollars and game minutes.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Tip Config", fileName = "TipConfig")]
    public sealed class TipConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _speedBonusWindowMinutes = 10f;
        [SerializeField, Min(0)] private int _speedBonus = 2;
        [SerializeField, Min(0)] private int _impatientSpeedBonus = 6;
        [SerializeField, Min(0f)] private float _penaltyPerMinute = 0.5f;
        [SerializeField, Min(0), Tooltip("Dollars added for an EV handed back fully charged.")]
        private int _chargeBonus = 15;
        [SerializeField, Min(0), Tooltip("Dollars added for a dirty car that was washed.")]
        private int _cleanBonus = 15;
        [SerializeField, Min(0), Tooltip("Dollars added when the patron still has all their patience left; scales down with the patience spent.")]
        private int _maxPatienceBonus = 10;
        [SerializeField, Min(0), Tooltip("Dollars added for each Perfect Park earned on the ticket (before damage and combo).")]
        private int _perfectParkBonus = 2;
        [SerializeField, Range(0.05f, 1f), Tooltip("Damage (0..1) per damage stage. 0.25 gives stages at 25%, 50%, 75% and 100%.")]
        private float _damageStageSize = 0.25f;
        [SerializeField, Range(0f, 1f), Tooltip("Share of the tip lost per damage stage. At 100% damage the tip is always $0.")]
        private float _damagePenaltyPerStage = 0.25f;

        public float SpeedBonusWindowMinutes => _speedBonusWindowMinutes;
        public int SpeedBonus => _speedBonus;
        public int ImpatientSpeedBonus => _impatientSpeedBonus;
        public float PenaltyPerMinute => _penaltyPerMinute;
        public int ChargeBonus => _chargeBonus;
        public int CleanBonus => _cleanBonus;
        public int MaxPatienceBonus => _maxPatienceBonus;
        public int PerfectParkBonus => _perfectParkBonus;
        public float DamageStageSize => _damageStageSize;
        public float DamagePenaltyPerStage => _damagePenaltyPerStage;
    }
}
