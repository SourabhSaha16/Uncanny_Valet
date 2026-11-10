using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Pure maths for cutting the diner's roof and facade out of one tall MetroCity building sprite and scaling them to the diner.</summary>
    public static class DinerSlices
    {
        /// <summary>Texture rectangle (origin bottom-left) of a pixel rectangle given from the top-left of the image.</summary>
        public static Rect TextureRect(RectInt fromTop, int textureHeight)
            => new Rect(fromTop.x, textureHeight - fromTop.y - fromTop.height, fromTop.width, fromTop.height);

        /// <summary>Pixels per world unit that make a slice <paramref name="pixelWidth"/> wide span <paramref name="worldWidth"/>.</summary>
        public static float PixelsPerUnit(int pixelWidth, float worldWidth) => pixelWidth / Mathf.Max(0.01f, worldWidth);

        /// <summary>World height of <paramref name="bands"/> stacked facade slices, never more than <paramref name="maxHeight"/> (whole bands only).</summary>
        public static float FacadeHeight(int bands, int pixelHeight, float pixelsPerUnit, float maxHeight)
        {
            var band = pixelHeight / pixelsPerUnit;
            var fit = Mathf.Min(Mathf.Max(0, bands), Mathf.FloorToInt(maxHeight / band));
            return fit * band;
        }
    }
}
