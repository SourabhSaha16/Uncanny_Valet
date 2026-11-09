using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UncannyValet.EditorTools.Import
{
    /// <summary>Builds deterministic sprite rectangles: grid cells {file}_r{row}_c{col}, auto pieces {file}_{index}.</summary>
    public static class SpriteSheetBuilder
    {
        private const int AutoMinSize = 8;
        private const float AutoRowBand = 64f;

        /// <summary>Cuts a w x h texture into cells; row 0 is the top row. Cells that do not fit are skipped.</summary>
        public static SpriteMetaData[] Grid(string file, int w, int h, int cw, int ch, Vector2 pivot)
        {
            var list = new List<SpriteMetaData>();
            for (int row = 0; (row + 1) * ch <= h; row++)
            {
                for (int col = 0; (col + 1) * cw <= w; col++)
                {
                    var rect = new Rect(col * cw, h - (row + 1) * ch, cw, ch);
                    list.Add(Make($"{file}_r{row}_c{col}", rect, pivot));
                }
            }
            return list.ToArray();
        }

        /// <summary>Detects opaque islands, orders them top-to-bottom then left-to-right and numbers them.</summary>
        public static SpriteMetaData[] Auto(string file, Texture2D tex, Vector2 pivot)
        {
            Rect[] rects = InternalSpriteUtility.GenerateAutomaticSpriteRectangles(tex, AutoMinSize, 0);
            float height = tex.height;
            Array.Sort(rects, (a, b) =>
            {
                int bandA = Mathf.FloorToInt((height - a.center.y) / AutoRowBand);
                int bandB = Mathf.FloorToInt((height - b.center.y) / AutoRowBand);
                return bandA != bandB ? bandA.CompareTo(bandB) : a.x.CompareTo(b.x);
            });
            var result = new SpriteMetaData[rects.Length];
            for (int i = 0; i < rects.Length; i++) result[i] = Make($"{file}_{i:00}", rects[i], pivot);
            return result;
        }

        private static SpriteMetaData Make(string name, Rect rect, Vector2 pivot)
        {
            return new SpriteMetaData { name = name, rect = rect, alignment = (int)SpriteAlignment.Custom, pivot = pivot };
        }
    }
}
