using System;
using UnityEngine;

namespace UncannyValet.UI.Kit
{
    /// <summary>
    /// The icon bitmaps as code tables. Each character is a palette key (see <see cref="Keys"/>); '.' is clear.
    /// Icons meant to be tinted (the key tag) are drawn in white.
    /// </summary>
    public static class PixelIconData
    {
        /// <summary>Palette keys: ink, white, gold, dark gold, amber, cyan, red, grey, light grey, dark, green.</summary>
        public const string Keys = "kwyoacrgldn";

        public static readonly Color32[] Colors =
        {
            new Color32(0x1A, 0x16, 0x26, 255), new Color32(255, 255, 255, 255),
            new Color32(0xFF, 0xC8, 0x3A, 255), new Color32(0xC9, 0x8A, 0x1E, 255),
            new Color32(0xFF, 0xB0, 0x20, 255), new Color32(0x38, 0xD6, 0xF0, 255),
            new Color32(0xE5, 0x48, 0x3C, 255), new Color32(0x8C, 0x8C, 0x9C, 255),
            new Color32(0xC8, 0xC8, 0xD4, 255), new Color32(0x4A, 0x42, 0x58, 255),
            new Color32(0x4C, 0xCB, 0x5E, 255),
        };

        private static readonly string[] Coin =
        {
            "..kkkk..", ".kyyyyk.", "kyywyyok", "kywyyyok", "kywyyyok", "kyyyyyok", ".kooook.", "..kkkk..",
        };

        private static readonly string[] StarLit =
        {
            "....k....", "...kyk...", "...kyk...", "kkkkykkkk", "kyyyyyyyk",
            ".kyyyyyk.", ".kyyyyyk.", "kyyykyyyk", "kkkk.kkkk",
        };

        private static readonly string[] StarUnlit = Recolor(StarLit, 'y', 'd');

        private static readonly string[] Bell =
        {
            "....k....", "...kyk...", "..kyyyk..", "..kywyk..", ".kywyyok.",
            ".kywyyok.", "kyyyyyyok", "kkkkkkkkk", "...kyk...",
        };

        private static readonly string[] KeyTag =
        {
            "..kkk..", ".kwwwk.", "kwwkwwk", "kwwwwwk", "kwwwwwk", "kwwwwwk", "kwwwwwk", ".kwwwk.", "..kkk..",
        };

        private static readonly string[] Car =
        {
            "...kkkkk...", "..kwwwwwk..", ".kwccwccwk.", "kwwwwwwwwwk",
            "kwwwwwwwwwk", ".kkgkkkgkk.", "..kgk.kgk..",
        };

        private static readonly string[] Wash =
        {
            "....k....", "...kck...", "..kcwck..", "..kcwcck.", ".kcwcccok",
            ".kccccook", ".kcccccok", "..kcccok.", "...kkk...",
        };

        private static readonly string[] Tyre =
        {
            "..kkkkk..", ".kgggggk.", "kgggggggk", "kggkkkggk", "kggklkggk",
            "kggkkkggk", "kgggggggk", ".kgggggk.", "..kkkkk..",
        };

        private static readonly string[] Plug =
        {
            "..k...k..", "..y...y..", "..y...y..", ".kkkkkkk.", ".knnnnnk.",
            ".knnnnnk.", "..knnnk..", "...knk...", "...kkk...",
        };

        /// <summary>The bitmap rows of an icon, top row first.</summary>
        public static string[] Rows(PixelIcon icon)
        {
            switch (icon)
            {
                case PixelIcon.Coin: return Coin;
                case PixelIcon.StarLit: return StarLit;
                case PixelIcon.StarUnlit: return StarUnlit;
                case PixelIcon.Bell: return Bell;
                case PixelIcon.KeyTag: return KeyTag;
                case PixelIcon.Car: return Car;
                case PixelIcon.Wash: return Wash;
                case PixelIcon.Tyre: return Tyre;
                case PixelIcon.Plug: return Plug;
                default: throw new ArgumentOutOfRangeException(nameof(icon), icon, "Icon has no bitmap.");
            }
        }

        private static string[] Recolor(string[] rows, char from, char to)
        {
            var result = new string[rows.Length];
            for (var i = 0; i < rows.Length; i++) result[i] = rows[i].Replace(from, to);
            return result;
        }
    }
}
