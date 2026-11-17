using System;
using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Watchers
{
    /// <summary>
    /// The sprite recipe of a watcher: which MetroCity pedestrian layers it starts from (skin row, hair row, outfit), the colour tint of each
    /// layer, the accessories drawn on top, and the idle bob. No image files: everything is recoloured or drawn in code.
    /// </summary>
    [Serializable]
    public sealed class WatcherLook
    {
        [SerializeField, Min(0), Tooltip("Row of the character model sheet (the skin).")] private int _skin;
        [SerializeField, Min(0), Tooltip("Row of the hair sheet.")] private int _hair;
        [SerializeField, Min(0), Tooltip("Outfit sheet number, counted from 0.")] private int _outfit;
        [SerializeField, Tooltip("Multiplies the skin layer's colours; white keeps them.")] private Color _skinTint = Color.white;
        [SerializeField, Tooltip("Multiplies the hair layer's colours; white keeps them.")] private Color _hairTint = Color.white;
        [SerializeField, Tooltip("Multiplies the outfit layer's colours; white keeps them.")] private Color _outfitTint = Color.white;
        [SerializeField, Tooltip("Rectangles drawn over the character (dress pattern, glasses, a clipboard).")]
        private List<WatcherAccessory> _accessories = new List<WatcherAccessory>();
        [SerializeField, Range(0f, 0.2f), Tooltip("Height of the idle bob, world units before scaling.")] private float _bobHeight = 0.03f;
        [SerializeField, Range(0.1f, 4f), Tooltip("Idle bobs per second.")] private float _bobSpeed = 1.2f;

        public WatcherLook(int skin, int hair, int outfit, Color skinTint, Color hairTint, Color outfitTint, List<WatcherAccessory> accessories)
        {
            _skin = skin; _hair = hair; _outfit = outfit;
            _skinTint = skinTint; _hairTint = hairTint; _outfitTint = outfitTint;
            _accessories = accessories ?? new List<WatcherAccessory>();
        }

        public int Skin => _skin;
        public int Hair => _hair;
        public int Outfit => _outfit;
        public Color SkinTint => _skinTint;
        public Color HairTint => _hairTint;
        public Color OutfitTint => _outfitTint;
        public IReadOnlyList<WatcherAccessory> Accessories => _accessories;
        public float BobHeight => _bobHeight;
        public float BobSpeed => _bobSpeed;

        /// <summary>
        /// True when the recipe can be drawn from a sprite set with the given row counts; otherwise <paramref name="problem"/> names the fault.
        /// Pass the pedestrian config's skin, hair and outfit counts.
        /// </summary>
        public bool TryValidate(int skins, int hairs, int outfits, out string problem)
        {
            problem = null;
            if (_skin >= skins) problem = "skin row " + _skin + " does not exist (there are " + skins + ")";
            else if (_hair >= hairs) problem = "hair row " + _hair + " does not exist (there are " + hairs + ")";
            else if (_outfit >= outfits) problem = "outfit " + _outfit + " does not exist (there are " + outfits + ")";
            else if (_accessories == null || _accessories.Count == 0) problem = "no accessories (every watcher is told apart by at least one)";
            for (var i = 0; problem == null && i < _accessories.Count; i++)
                if (_accessories[i] == null || _accessories[i].SizePixels.x <= 0f || _accessories[i].SizePixels.y <= 0f || _accessories[i].Color.a <= 0f)
                    problem = "accessory " + i + " has no size or is transparent";
            return problem == null;
        }
    }
}
