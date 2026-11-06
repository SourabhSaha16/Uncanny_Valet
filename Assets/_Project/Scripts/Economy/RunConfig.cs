using UnityEngine;

namespace UncannyValet.Economy
{
    /// <summary>The run's length and debt (design.md section 2). Placeholders: X nights, Y dollars.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Run Config", fileName = "RunConfig")]
    public sealed class RunConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int _nights = 7;
        [SerializeField, Min(0)] private int _startingDebt = 1500;

        public int Nights => _nights;
        public int StartingDebt => _startingDebt;
    }
}
