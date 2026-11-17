using System;
using UncannyValet.UI.Kit;
using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>
    /// The pie art, generated once: a dark disc with its outline (the back) and one white wedge sprite per step, all cut from one
    /// point-filtered strip texture at 32 pixels per world unit. The wedge is white so a renderer colour tints it.
    /// </summary>
    public sealed class PickupPieSprites : IDisposable
    {
        /// <summary>Sprite pixels per world unit, the same as the rest of the world art.</summary>
        public const float PixelsPerUnit = 32f;

        private readonly Texture2D _texture;
        private readonly Sprite[] _wedges;

        public PickupPieSprites(int diameterPixels, int steps)
        {
            var size = diameterPixels;
            _texture = new Texture2D(size * (steps + 1), size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "PickupPie" };
            var pixels = new Color32[_texture.width * size];
            for (var step = 0; step < steps; step++) PaintWedge(pixels, step * size, size, step / (float)steps);
            PaintBack(pixels, steps * size, size);
            _texture.SetPixels32(pixels);
            _texture.Apply(false, true);
            _wedges = new Sprite[steps];
            for (var step = 0; step < steps; step++) _wedges[step] = Cut(step * size, size);
            Back = Cut(steps * size, size);
        }

        /// <summary>The dark disc with its outline, drawn behind the wedge.</summary>
        public Sprite Back { get; }

        /// <summary>The white wedge left after cutting away the given step (0 = full pie); null past the last step (empty).</summary>
        public Sprite Wedge(int step) => step >= 0 && step < _wedges.Length ? _wedges[step] : null;

        public void Dispose()
        {
            for (var i = 0; i < _wedges.Length; i++) PieObjects.Release(_wedges[i]);
            PieObjects.Release(Back);
            PieObjects.Release(_texture);
        }

        private Sprite Cut(int left, int size)
            => Sprite.Create(_texture, new Rect(left, 0, size, size), new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect);

        private static void PaintWedge(Color32[] pixels, int left, int size, float cutFraction)
        {
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    if (PiePixels.InWedge(x, y, size, cutFraction)) pixels[y * (pixels.Length / size) + left + x] = Color.white;
        }

        private static void PaintBack(Color32[] pixels, int left, int size)
        {
            Color32 ink = PixelPalette.Ink;
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    if (PiePixels.InDisc(x, y, size)) pixels[y * (pixels.Length / size) + left + x] = ink;
        }
    }
}
