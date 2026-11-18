using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Creates the walk links <see cref="WalkAutoLinker"/> proposes, as undoable objects under the lot.</summary>
    public static class WalkLinkCreator
    {
        /// <summary>Links the nodes of <paramref name="lot"/> that lie within <paramref name="maxDistance"/> of each other; returns how many links were made.</summary>
        public static int Create(HandmadeLot lot, float maxDistance)
        {
            var nodes = lot.GetComponentsInChildren<WalkNode>(true);
            var plan = WalkAutoLinker.Plan(HandmadeLotReader.Read(lot), maxDistance);
            foreach (var (a, b) in plan) Add(lot.transform, nodes[a], nodes[b]);
            return plan.Count;
        }

        private static void Add(Transform parent, WalkNode a, WalkNode b)
        {
            var go = new GameObject("WalkLink " + a.name + " - " + b.name);
            Undo.RegisterCreatedObjectUndo(go, "Auto-link walk nodes");
            go.transform.SetParent(parent, false);
            var link = go.AddComponent<WalkLink>();
            var so = new SerializedObject(link);
            so.FindProperty("_a").objectReferenceValue = a;
            so.FindProperty("_b").objectReferenceValue = b;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
