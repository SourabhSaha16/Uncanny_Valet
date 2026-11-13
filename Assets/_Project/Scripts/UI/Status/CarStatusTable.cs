using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Status
{
    /// <summary>Label, short label and colour of every <see cref="CarStatus"/>, in enum order.</summary>
    public static class CarStatusTable
    {
        private readonly struct Entry
        {
            public readonly string Label;
            public readonly string Short;
            public readonly Color Color;

            public Entry(string label, string shortLabel, Color color)
            {
                Label = label;
                Short = shortLabel;
                Color = color;
            }
        }

        private static readonly Entry[] Entries =
        {
            new Entry("Parked", "Parked", PixelPalette.Cyan),
            new Entry("Servicing", "Servicing", PixelPalette.Cyan),
            new Entry("Serviced", "Serviced", PixelPalette.Green),
            new Entry("Pick Up", "Pick Up", PixelPalette.Amber),
            new Entry("Cust. Angry", "Angry", PixelPalette.OrangeRed),
            new Entry("Unblessed", "Zero", PixelPalette.Grey)
        };

        /// <summary>Number of statuses the table covers; it must equal the enum's length.</summary>
        public static int Count => Entries.Length;

        /// <summary>The full label of the status.</summary>
        public static string Label(CarStatus status) => Entries[(int)status].Label;

        /// <summary>The status colour.</summary>
        public static Color ColorOf(CarStatus status) => Entries[(int)status].Color;

        /// <summary>
        /// The label of the status for this row: the service statuses say the word of the modifier involved (Washing,
        /// Charged, ...), the others their table label; the full label when it fits, else the short one.
        /// </summary>
        public static string LabelFitting(CarStatus status, TicketRow row, int availablePixels)
        {
            var word = ServiceWord(status, row);
            return word.Length > 0 ? word : LabelFitting(status, availablePixels);
        }

        private static string ServiceWord(CarStatus status, TicketRow row)
        {
            if (status == CarStatus.Servicing) return row.HasStation ? row.OpenAt(row.Station)?.ActiveWord ?? string.Empty : string.Empty;
            if (status == CarStatus.Serviced) return row.JustCleared()?.DoneWord ?? string.Empty;
            return string.Empty;
        }

        /// <summary>The full label when it fits in <paramref name="availablePixels"/>, else the short one.</summary>
        public static string LabelFitting(CarStatus status, int availablePixels)
        {
            var entry = Entries[(int)status];
            return PixelTextLayout.Width(entry.Label.Length, 1) <= availablePixels ? entry.Label : entry.Short;
        }
    }
}
