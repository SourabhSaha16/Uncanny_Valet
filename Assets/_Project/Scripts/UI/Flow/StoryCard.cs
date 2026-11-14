using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.UI.Flow
{
    /// <summary>One ready-to-draw line of a story card: wrapped, positioned and coloured.</summary>
    public readonly struct CardLine
    {
        public readonly string Text;
        public readonly int X;
        public readonly int Y;
        public readonly int Scale;
        public readonly Color Color;

        public CardLine(string text, int x, int y, int scale, Color color)
        {
            Text = text; X = x; Y = y; Scale = scale; Color = color;
        }
    }

    /// <summary>
    /// A story card laid out once when its screen starts (so drawing allocates nothing): the lines, the world's accent colours and, on a reward
    /// card, the power icon.
    /// </summary>
    public sealed class StoryCard
    {
        public readonly List<CardLine> Lines = new List<CardLine>();
        public Color Accent;
        public Color AccentSecond;
        public string Prompt;
        public bool HasIcon;
        public StoryPower IconPower;
        public int IconX;
        public int IconY;
    }
}
