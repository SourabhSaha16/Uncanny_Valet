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
            new Entry("Washing", "Washing", PixelPalette.Cyan),
            new Entry("Washed", "Washed", PixelPalette.Green),
            new Entry("Charging", "Charging", PixelPalette.Cyan),
            new Entry("Charged", "Charged", PixelPalette.Green),
            new Entry("Pick Up", "Pick Up", PixelPalette.Amber),
            new Entry("Cust. Angry", "Angry", PixelPalette.OrangeRed)
        };

        /// <summary>Number of statuses the table covers; it must equal the enum's length.</summary>
        public static int Count => Entries.Length;

        /// <summary>The full label of the status.</summary>
        public static string Label(CarStatus status) => Entries[(int)status].Label;

        /// <summary>The status colour.</summary>
        public static Color ColorOf(CarStatus status) => Entries[(int)status].Color;

        /// <summary>The full label when it fits in <paramref name="availablePixels"/>, else the short one.</summary>
        public static string LabelFitting(CarStatus status, int availablePixels)
        {
            var entry = Entries[(int)status];
            return PixelTextLayout.Width(entry.Label.Length, 1) <= availablePixels ? entry.Label : entry.Short;
        }
    }
}
