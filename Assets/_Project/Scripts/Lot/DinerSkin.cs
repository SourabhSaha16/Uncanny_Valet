using UncannyValet.Core.Art;
using UnityEngine;
using UnityEngine.Rendering;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Re-skins the MetroCity diner building with the world palette (tan walls, green roof for Midgard): reads back just the slice it needs
    /// from the unreadable import, recolours it once with <see cref="PixelRecolour"/> and makes a point filtered sprite.
    /// </summary>
    public static class DinerSkin
    {
        /// <summary>The recoloured slice (pixels from the texture's bottom-left), or null when no graphics device can read it back.</summary>
        public static Sprite Slice(Texture2D source, Rect textureRect, float pixelsPerUnit, WorldPalette palette)
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return null;
            var pixels = ReadBack(source, textureRect);
            PixelRecolour.Apply(pixels, palette.Get(PaletteSlot.Roof), palette.Get(PaletteSlot.Wall));
            var width = Mathf.RoundToInt(textureRect.width);
            var height = Mathf.RoundToInt(textureRect.height);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "Diner Skin" };
            texture.SetPixels32(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Color32[] ReadBack(Texture2D source, Rect area)
        {
            var target = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            Graphics.Blit(source, target);
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var copy = new Texture2D(Mathf.RoundToInt(area.width), Mathf.RoundToInt(area.height), TextureFormat.RGBA32, false);
            copy.ReadPixels(area, 0, 0);
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
            var pixels = copy.GetPixels32();
            Object.Destroy(copy);
            return pixels;
        }
    }
}
