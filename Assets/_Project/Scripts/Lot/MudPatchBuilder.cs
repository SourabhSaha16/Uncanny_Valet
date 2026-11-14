using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Builds the lot's mud puddles from the patch list: one sprite per puddle on a "Mud Patches" group under the lot (no colliders,
    /// cars and walkers drive and walk over them), and registers <see cref="IMudPatches"/> so cars can feel them. A lot without
    /// patches builds nothing and registers nothing.
    /// </summary>
    public static class MudPatchBuilder
    {
        /// <summary>Above the bay lines, below the skid marks.</summary>
        public const int SortingOrder = -4;

        /// <summary>Builds the puddles; returns the field, or null when the list is empty.</summary>
        public static MudPatchField Build(Transform lot, IReadOnlyList<MudPatch> patches)
        {
            if (patches == null || patches.Count == 0) return null;
            var art = new MudBlobArt();
            var group = new GameObject("Mud Patches").transform;
            group.SetParent(lot, false);
            for (var i = 0; i < patches.Count; i++)
            {
                var go = new GameObject("Mud Patch " + i);
                go.transform.SetParent(group, false);
                go.transform.localPosition = patches[i].Center;
                go.transform.localScale = new Vector3(patches[i].Size, patches[i].Size, 1f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = art.Get(i);
                renderer.sortingOrder = SortingOrder;
            }
            var field = new MudPatchField(patches, lot.position);
            Services.Register<IMudPatches>(field);
            return field;
        }
    }
}
