using System;
using UnityEngine;

namespace UncannyValet.Core.Watchers
{
    /// <summary>
    /// One accessory drawn in code on top of a watcher's recoloured pedestrian: a plain coloured rectangle (a clipboard, a lens, a stripe).
    /// Positions are in pixels of the 32 by 32 character frame, from its top-left corner (x right, y down).
    /// </summary>
    [Serializable]
    public sealed class WatcherAccessory
    {
        [SerializeField, Tooltip("What it is, for the editor only.")] private string _label;
        [SerializeField, Tooltip("Centre in frame pixels from the top-left corner (the body is centred at x 16; the head is near y 9, the feet near y 28).")]
        private Vector2 _centerPixels;
        [SerializeField, Tooltip("Width and height in frame pixels.")] private Vector2 _sizePixels = Vector2.one;
        [SerializeField] private Color _color = Color.white;
        [SerializeField, Tooltip("Draw order over the hair layer: 0 = just over the hair, higher draws later.")]
        private int _order;

        public WatcherAccessory(string label, Vector2 centerPixels, Vector2 sizePixels, Color color, int order = 0)
        {
            _label = label; _centerPixels = centerPixels; _sizePixels = sizePixels; _color = color; _order = order;
        }

        public string Label => _label;
        public Vector2 CenterPixels => _centerPixels;
        public Vector2 SizePixels => _sizePixels;
        public Color Color => _color;
        public int Order => _order;
    }
}
