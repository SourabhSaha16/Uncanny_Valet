using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>The painted facing arrows of the pickup zone: recolours their fill strokes for the current state.</summary>
    public sealed class CurbArrow : MonoBehaviour
    {
        private readonly List<SpriteRenderer> _fills = new List<SpriteRenderer>();
        private CurbArrowStyle _style;

        /// <summary>Sets the style whose state colours are used.</summary>
        public void Init(CurbArrowStyle style) { _style = style; }

        /// <summary>Registers a fill stroke to recolour.</summary>
        public void AddFill(SpriteRenderer fill) { _fills.Add(fill); }

        /// <summary>Tints every fill stroke for <paramref name="state"/>.</summary>
        public void Show(CurbArrowState state)
        {
            var color = _style.ColorOf(state);
            for (var i = 0; i < _fills.Count; i++) _fills[i].color = color;
        }
    }
}
