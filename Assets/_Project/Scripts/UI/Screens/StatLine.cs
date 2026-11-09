using UncannyValet.UI.Kit;

namespace UncannyValet.UI.Screens
{
    /// <summary>One stats line on the results screen: a small icon, a label and a value.</summary>
    public readonly struct StatLine
    {
        public readonly PixelIcon Icon;
        public readonly string Label;
        public readonly string Value;

        public StatLine(PixelIcon icon, string label, string value)
        {
            Icon = icon; Label = label; Value = value;
        }
    }
}
