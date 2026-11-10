using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure rules for the pickup mark on a key board row: a highlight and a bell that flash with the patron's emotion,
    /// the same colour and speed as the plate (green slow, yellow faster, red very fast).
    /// </summary>
    public static class PickupMark
    {
        /// <summary>True when the row's car is wanted back, so the row shows the mark.</summary>
        public static bool IsRequested(TicketRow row) => row.State == TicketRowState.PickupRequested;

        /// <summary>True during the lit half of the current flash.</summary>
        public static bool IsLit(TicketRow row, float time, EmotionStyle style)
            => style.IsLit(style.Rule.Resolve(row), time);

        /// <summary>Highlight colour: the emotion colour.</summary>
        public static Color Tint(TicketRow row, EmotionStyle style) => style.ColorOf(style.Rule.Resolve(row));
    }
}
