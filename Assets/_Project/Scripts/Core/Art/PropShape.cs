using System;
using UnityEngine;

namespace UncannyValet.Core.Art
{
    /// <summary>One flat-filled shape of a prop layer, in pixels from the bottom-left corner of the prop picture, with a colour from the world palette.</summary>
    [Serializable]
    public struct PropShape
    {
        [SerializeField] private PropShapeKind _kind;
        [SerializeField, Tooltip("Left, bottom, width and height in pixels (32 pixels are one world unit).")]
        private RectInt _area;
        [SerializeField, Tooltip("Which colour of the world palette fills the shape.")]
        private PaletteSlot _colour;
        [SerializeField, Tooltip("The letters of a Text shape (the pixel font has a few letters only).")]
        private string _text;

        public PropShape(PropShapeKind kind, RectInt area, PaletteSlot colour, string text = null)
        {
            _kind = kind;
            _area = area;
            _colour = colour;
            _text = text;
        }

        public PropShapeKind Kind => _kind;
        public RectInt Area => _area;
        public PaletteSlot Colour => _colour;
        public string Text => _text;
    }
}
