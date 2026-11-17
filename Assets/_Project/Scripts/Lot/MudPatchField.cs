using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>The mud puddles of a lot in world space: answers whether a point is in mud.</summary>
    public sealed class MudPatchField : IMudPatches
    {
        private readonly MudPatch[] _patches;

        /// <param name="patches">Puddles in the lot's own space.</param>
        /// <param name="origin">World position of the lot's origin.</param>
        public MudPatchField(IReadOnlyList<MudPatch> patches, Vector2 origin)
        {
            _patches = new MudPatch[patches.Count];
            for (var i = 0; i < _patches.Length; i++) _patches[i] = patches[i].Moved(origin);
        }

        /// <summary>Number of puddles.</summary>
        public int Count => _patches.Length;

        public bool IsInside(Vector2 point)
        {
            for (var i = 0; i < _patches.Length; i++)
                if (_patches[i].Contains(point)) return true;
            return false;
        }
    }
}
