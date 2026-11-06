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

        public float SpeedBonusWindowMinutes => _speedBonusWindowMinutes;
        public int SpeedBonus => _speedBonus;
        public int ImpatientSpeedBonus => _impatientSpeedBonus;
        public float PenaltyPerMinute => _penaltyPerMinute;
    }
}
