using UnityEngine;

namespace UncannyValet.Core.Art
{
    /// <summary>
    /// The small colour set of one world (about twelve colours: ground, wall, roof, accent, light, shadow and so on) that all of that
    /// world's props are drawn with. Day and night looks are lighting, not new palettes. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Art/World Palette", fileName = "WorldPalette")]
    public sealed class WorldPalette : ScriptableObject
    {
        /// <summary>Number of colours every palette holds (one per <see cref="PaletteSlot"/>).</summary>
        public const int SlotCount = 12;

        [SerializeField, Tooltip("One colour per slot, in the order of the PaletteSlot enum (Outline, Ground, Wall, Roof, Accent, Light, Shadow, Trim, Plant, Wood, Metal, Sky).")]
        private Color[] _colors = new Color[SlotCount];

        /// <summary>The colour of a slot; transparent when the palette is incomplete (see <see cref="TryValidate"/>).</summary>
        public Color32 Get(PaletteSlot slot)
        {
            var index = (int)slot;
            return _colors != null && index >= 0 && index < _colors.Length ? (Color32)_colors[index] : default;
        }

        /// <summary>True when every slot has an opaque colour; otherwise <paramref name="problem"/> names the first fault.</summary>
        public bool TryValidate(out string problem)
        {
            problem = null;
            if (_colors == null || _colors.Length != SlotCount) problem = name + ": the palette needs exactly " + SlotCount + " colours";
            for (var i = 0; problem == null && i < SlotCount; i++)
                if (_colors[i].a < 1f) problem = name + ": colour " + (PaletteSlot)i + " is not opaque";
            return problem == null;
        }

        /// <summary>Makes a palette from colours in slot order (used by the setup tool and tests).</summary>
        public static WorldPalette Create(string paletteName, Color[] colors)
        {
            var palette = CreateInstance<WorldPalette>();
            palette.name = paletteName;
            palette._colors = colors;
            return palette;
        }
    }
}
