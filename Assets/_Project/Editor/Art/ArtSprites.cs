using System.IO;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>Finds MetroCity sprites by name or by a pixel inside them, and measures what is drawn in them. Editor only.</summary>
    internal static class ArtSprites
    {
        public const string Root = "Assets/_Project/Art/ThirdParty/MetroCity/";
        private const float PointSlack = 4f;

        /// <summary>The sprite called <paramref name="spriteName"/> in the PNG under the MetroCity root, or null (logged).</summary>
        public static Sprite Named(string path, string spriteName)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(Root + path))
                if (asset is Sprite sprite && sprite.name == spriteName) return sprite;
            Debug.LogError("ArtSprites: sprite '" + spriteName + "' not found in " + Root + path + ". Re-import the art.");
            return null;
        }

        /// <summary>The sprite containing the pixel (x, y counted from the top-left), or the nearest within a few pixels.</summary>
        public static Sprite AtPoint(string path, int x, int yFromTop)
        {
            Sprite best = null;
            var bestDistance = float.MaxValue;
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(Root + path))
            {
                if (!(asset is Sprite sprite)) continue;
                var point = new Vector2(x + 0.5f, sprite.texture.height - yFromTop - 0.5f);
                var distance = Vector2.Distance(point, Vector2.Max(Vector2.Min(point, sprite.rect.max), sprite.rect.min));
                if (distance < bestDistance) { best = sprite; bestDistance = distance; }
            }
            if (best == null || bestDistance > PointSlack)
                Debug.LogWarning("ArtSprites: no sprite near (" + x + ", " + yFromTop + ") in " + path);
            return bestDistance > PointSlack ? null : best;
        }

        /// <summary>Colour of the pixel (x, y from the top-left) of the PNG.</summary>
        public static Color PixelColor(string path, int x, int yFromTop)
        {
            var tex = Decode(Root + path);
            var color = tex.GetPixel(x, tex.height - 1 - yFromTop);
            Object.DestroyImmediate(tex);
            color.a = 1f;
            return color;
        }

        /// <summary>
        /// Size of the opaque pixels of a sprite and their centre relative to the sprite pivot, in world units.
        /// </summary>
        public static bool TryMeasure(Sprite sprite, string path, out Vector2 size, out Vector2 center)
        {
            size = Vector2.one;
            center = Vector2.zero;
            if (sprite == null) return false;
            var tex = Decode(Root + path);
            var pixels = tex.GetPixels32();
            var rect = sprite.rect;
            int x0 = int.MaxValue, y0 = int.MaxValue, x1 = -1, y1 = -1;
            for (var y = (int)rect.yMin; y < (int)rect.yMax; y++)
            for (var x = (int)rect.xMin; x < (int)rect.xMax; x++)
            {
                if (pixels[y * tex.width + x].a == 0) continue;
                x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x);
                y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y);
            }
            var ppu = sprite.pixelsPerUnit;
            Object.DestroyImmediate(tex);
            if (x1 < 0) return false;
            size = new Vector2(x1 - x0 + 1, y1 - y0 + 1) / ppu;
            var middle = new Vector2((x0 + x1 + 1) * 0.5f, (y0 + y1 + 1) * 0.5f);
            center = (middle - rect.min - sprite.pivot) / ppu;
            return true;
        }

        private static Texture2D Decode(string assetPath)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(assetPath));
            return tex;
        }
    }
}
