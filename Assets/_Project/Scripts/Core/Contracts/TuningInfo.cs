using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Display name and slider range of one tuning key.</summary>
    public readonly struct TuningInfo
    {
        public readonly TuningKey Key;
        public readonly string Group;
        public readonly string Label;
        public readonly float Min;
        public readonly float Max;
        public readonly float Step;

        public TuningInfo(TuningKey key, string group, string label, float min, float max, float step)
        {
            Key = key; Group = group; Label = label; Min = min; Max = max; Step = step;
        }
    }
}
