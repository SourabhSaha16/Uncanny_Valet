using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Display name, slider range and designer help of one tuning key.</summary>
    public readonly struct TuningInfo
    {
        /// <summary>Group heading of the dials the panel shows first and always.</summary>
        public const string DifficultyGroup = "Difficulty";

        public readonly TuningKey Key;
        public readonly string Group;
        public readonly string Label;
        public readonly float Min;
        public readonly float Max;
        public readonly float Step;
        /// <summary>One plain-language line for a game designer; empty when the label says enough.</summary>
        public readonly string Tooltip;
        /// <summary>True when the row only shows after the panel's Advanced toggle is switched on.</summary>
        public readonly bool Advanced;

        public TuningInfo(TuningKey key, string group, string label, float min, float max, float step,
            string tooltip = "", bool advanced = true)
        {
            Key = key; Group = group; Label = label; Min = min; Max = max; Step = step;
            Tooltip = tooltip ?? string.Empty; Advanced = advanced;
        }
    }
}
