using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>Tunable patron numbers (design.md section 5). Times are game minutes.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Patron Config", fileName = "PatronConfig")]
    public class PatronConfig : ScriptableObject
    {
        [SerializeField] private Vector2 _arrivalIntervalRange = new Vector2(8f, 15f);
        [SerializeField] private Vector2 _stayRange = new Vector2(20f, 45f);
        [SerializeField] private int _baseTip = 20;
        [SerializeField, Range(0f, 1f)] private float _impatientChance = 0.25f;

        public Vector2 ArrivalIntervalRange => _arrivalIntervalRange;
        public Vector2 StayRange => _stayRange;
        public int BaseTip => _baseTip;
        public float ImpatientChance => _impatientChance;
    }
}
