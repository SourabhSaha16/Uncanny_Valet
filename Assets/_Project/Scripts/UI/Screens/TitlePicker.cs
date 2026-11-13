using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.UI.Screens
{
    /// <summary>
    /// The temporary developer lot picker on the title screen: one numbered line per lot of the run (keys 1 to 9,
    /// then 0). Pressing a digit loads that lot's scene. Hidden later when the story flow is adopted.
    /// </summary>
    public sealed class TitlePicker
    {
        /// <summary>Keys 1 to 9 and 0.</summary>
        public const int MaxEntries = 10;

        private const int KeyZeroNumber = 10;
        private readonly ILotPicker _picker;
        private readonly string[] _lines;

        public TitlePicker(ILotPicker picker)
        {
            _picker = picker;
            var choices = picker.Choices;
            _lines = new string[System.Math.Min(choices.Count, MaxEntries)];
            for (var i = 0; i < _lines.Length; i++) _lines[i] = Format(i, choices[i]);
        }

        /// <summary>Number of lines shown.</summary>
        public int Count => _lines.Length;

        /// <summary>The text of one line, for example "2 FAIR NIGHT AT GRANDMA'S (CRAZY)".</summary>
        public string Line(int index) => _lines[index];

        /// <summary>Starts the lot whose digit key went down this frame. True when a lot was chosen.</summary>
        public bool TryPick()
        {
            return UiInput.TryDigitPressed(out var index) && index < _lines.Length && _picker.TryChoose(index);
        }

        /// <summary>The line label: the digit key, the lot name and the kind tag.</summary>
        public static string Format(int index, LotChoice choice)
        {
            var key = (index + 1) % KeyZeroNumber;
            return key + " " + choice.Name.ToUpperInvariant() + " (" + KindTag(choice.Id.Kind) + ")";
        }

        private static string KindTag(LotKind kind) => kind == LotKind.Training ? "TRAINING" : kind == LotKind.Crazy ? "CRAZY" : "FINALE";
    }
}
