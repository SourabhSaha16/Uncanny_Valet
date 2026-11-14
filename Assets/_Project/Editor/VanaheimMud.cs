using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// The five mud patches of the storm feast lot: two on the aisle just past the bays and one on the approach to each trough, which are the
    /// shortest driving lines between the bays and the troughs. Sizes stay between 2.5 and 4 units.
    /// </summary>
    internal static class VanaheimMud
    {
        private const float FirstOffset = 3.3f;
        private const float SecondOffset = 7.8f;
        private const float ApproachBeforeTrough = 2.7f;
        private const float ApproachFromTrough = 2.6f;
        private static readonly float[] Sizes = { 3.5f, 4f, 3f, 3f, 3.5f };

        public static MudPatchModifier Create(LotGeometry g)
        {
            var bays = g.Spots[g.Spots.Length - 1].xMax;
            var near = g.Troughs[0].xMin - ApproachBeforeTrough;
            var patches = new List<MudPatch>
            {
                new MudPatch(new Vector2(bays + FirstOffset, g.AisleY), Sizes[0]),
                new MudPatch(new Vector2(bays + SecondOffset, g.AisleY), Sizes[1]),
                new MudPatch(new Vector2(near, g.Troughs[0].center.y - ApproachFromTrough), Sizes[2]),
                new MudPatch(new Vector2(near, g.Troughs[1].center.y + ApproachFromTrough), Sizes[3]),
                new MudPatch(new Vector2(near - 0.5f, g.Troughs[2].center.y + ApproachFromTrough), Sizes[4]),
            };
            var mud = ScriptableObject.CreateInstance<MudPatchModifier>();
            mud.name = "VanaheimMudPatches";
            var so = new SerializedObject(mud);
            so.FindProperty("_displayName").stringValue = "Mud patches";
            var list = so.FindProperty("_patches");
            list.arraySize = patches.Count;
            for (var i = 0; i < patches.Count; i++)
            {
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_center").vector2Value = patches[i].Center;
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_size").floatValue = patches[i].Size;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return mud;
        }
    }
}
