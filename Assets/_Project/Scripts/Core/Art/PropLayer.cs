using System;
using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Art
{
    /// <summary>
    /// One drawn part of a prop: shapes painted in order, optionally outlined, optionally moving. Parts that move (a wheel, a sign)
    /// are their own layer so they can turn or blink on their own while the rest stays still.
    /// </summary>
    [Serializable]
    public sealed class PropLayer
    {
        [SerializeField] private string _name = "Layer";
        [SerializeField] private List<PropShape> _shapes = new List<PropShape>();
        [SerializeField, Tooltip("Draw the one pixel dark outline around the layer.")]
        private bool _outlined = true;
        [SerializeField, Tooltip("Drawn with the unlit glow material so lights and night do not dim it (signs, bulbs).")]
        private bool _glows;
        [SerializeField] private PropAnimation _animation;
        [SerializeField, Tooltip("Spin: degrees per second (negative turns the other way). Blink: seconds for one on and off.")]
        private float _speed;
        [SerializeField, Tooltip("The point in pixels (from the bottom-left of the prop picture) a spinning layer turns around.")]
        private Vector2Int _pivot;
        [SerializeField, Tooltip("Added to the placement's sorting order so layers stack (higher is in front).")]
        private int _orderOffset;

        public string Name => _name;
        public IReadOnlyList<PropShape> Shapes => _shapes;
        public bool Outlined => _outlined;
        public bool Glows => _glows;
        public PropAnimation Animation => _animation;
        public float Speed => _speed;
        public Vector2Int Pivot => _pivot;
        public int OrderOffset => _orderOffset;

        /// <summary>Makes a layer (used by the setup tool and tests).</summary>
        public static PropLayer Create(string name, PropShape[] shapes, bool outlined = true, bool glows = false,
            PropAnimation animation = PropAnimation.None, float speed = 0f, Vector2Int pivot = default, int orderOffset = 0)
            => new PropLayer { _name = name, _shapes = new List<PropShape>(shapes), _outlined = outlined, _glows = glows,
                _animation = animation, _speed = speed, _pivot = pivot, _orderOffset = orderOffset };
    }
}
