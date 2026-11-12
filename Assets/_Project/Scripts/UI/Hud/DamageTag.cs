using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// The "DMG 30%" mark on a key board row. Shown only when the car has damage; it sits in its own column to the right of the
    /// bell, so the key board grows by <see cref="ExtraWidth"/> only while some listed car is damaged and no other column moves.
    /// </summary>
    public static class DamageTag
    {
        /// <summary>Pixels the panel grows by to hold the longest mark, "DMG 100%".</summary>
        public const int ExtraWidth = 54;

        private const string Label = "DMG";
        private const string Wrecked = "100";
        private const int FullPercent = 100;
        private const int LabelGap = 4;

        /// <summary>True when the row has damage to show.</summary>
        public static bool IsShown(TicketRow row) => row.Damage01 > 0f;

        /// <summary>The damage as a whole percent, at least 1 while there is any damage and at most 100.</summary>
        public static int Percent(float damage01)
            => damage01 <= 0f ? 0 : Mathf.Clamp(Mathf.RoundToInt(damage01 * FullPercent), 1, FullPercent);

        /// <summary>Draws "DMG nn%" at the position, in the warning colour, without allocating.</summary>
        public static void Draw(PixelCanvas canvas, TicketRow row, int x, int y)
        {
            var percent = Percent(row.Damage01);
            canvas.Text(Label, x, y, PixelPalette.OrangeRed);
            var numberX = x + PixelTextLayout.Width(Label.Length, 1) + LabelGap;
            var number = percent >= FullPercent ? Wrecked : SmallNumbers.Get(percent);
            canvas.Text(number, numberX, y, PixelPalette.OrangeRed);
            canvas.Char('%', numberX + number.Length * PixelTextLayout.Advance, y, PixelPalette.OrangeRed, 1);
        }
    }
}
