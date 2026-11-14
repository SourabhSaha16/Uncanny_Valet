using System;
using UnityEngine;

namespace UncannyValet.Core.Art
{
    /// <summary>One prop of a lot: which recipe, where (lot space, the centre of the prop) and how it sorts.</summary>
    [Serializable]
    public struct PropPlacement
    {
        /// <summary>Default sorting order: above the ground and bay lines, below cars.</summary>
        public const int DefaultOrder = 3;

        [SerializeField] private PropRecipe _recipe;
        [SerializeField, Tooltip("Centre of the prop in the lot's own space.")]
        private Vector2 _position;
        [SerializeField, Tooltip("Sorting order of the prop's first layer.")]
        private int _sortingOrder;

        public PropPlacement(PropRecipe recipe, Vector2 position, int sortingOrder = DefaultOrder)
        {
            _recipe = recipe;
            _position = position;
            _sortingOrder = sortingOrder;
        }

        public PropRecipe Recipe => _recipe;
        public Vector2 Position => _position;
        public int SortingOrder => _sortingOrder;
    }
}
