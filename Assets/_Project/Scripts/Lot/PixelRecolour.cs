using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure palette swap of a MetroCity picture: strongly red pixels (a roof) take a new colour and grey or white pixels (walls) take a tint,
    /// both keeping the pixel's own light and dark shading; everything else (windows, outlines) is left alone. Reusable for any recoloured
    /// MetroCity sprite, such as the watchers.
    /// </summary>
    public static class PixelRecolour
    {
        private const float RedHueLimit = 0.04f;
        private const float RedMinSaturation = 0.5f;
        private const float RedShadeReference = 0.8f;
        private const float GreyMaxSaturation = 0.12f;
        private const float GreyMinValue = 0.35f;
        private const float GreyShadeReference = 0.9f;
        private const float MinValue = 0.2f;

        /// <summary>The recoloured pixel: red becomes <paramref name="red"/>, grey becomes <paramref name="grey"/>, others stay.</summary>
        public static Color32 Remap(Color32 pixel, Color32 red, Color32 grey)
        {
            if (pixel.a == 0) return pixel;
            Color.RGBToHSV(pixel, out var hue, out var saturation, out var value);
            var isRed = saturation >= RedMinSaturation && value > MinValue && (hue <= RedHueLimit || hue >= 1f - RedHueLimit);
            if (isRed) return Shade(red, value / RedShadeReference, pixel.a);
            var isGrey = saturation <= GreyMaxSaturation && value >= GreyMinValue;
            return isGrey ? Shade(grey, value / GreyShadeReference, pixel.a) : pixel;
        }

        /// <summary>Recolours every pixel in place.</summary>
        public static void Apply(Color32[] pixels, Color32 red, Color32 grey)
        {
            for (var i = 0; i < pixels.Length; i++) pixels[i] = Remap(pixels[i], red, grey);
        }

        private static Color32 Shade(Color32 colour, float scale, byte alpha)
            => new Color32(Scale(colour.r, scale), Scale(colour.g, scale), Scale(colour.b, scale), alpha);

        private static byte Scale(byte channel, float scale) => (byte)Mathf.Clamp(Mathf.RoundToInt(channel * scale), 0, 255);
    }
}
