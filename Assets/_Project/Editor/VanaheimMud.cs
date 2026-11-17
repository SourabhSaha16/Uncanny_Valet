using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// The five mud patches of the storm feast lot, all on the way to the wash bays that stand right beside the bay block: two on the aisle
    /// before the last bay columns, one on the aisle in front of each of the first two wash bays and one on the open ground west of the third.
    /// Sizes stay between 2.5 and 4 units.
    /// </summary>
    internal static class VanaheimMud
    {
        private const float FirstBack = 3f;
        private const float SecondBack = 8f;
        private const float AisleSide = 1.5f;
        private const float WestOfLastWashBay = 2.5f;
        private static readonly float[] Sizes = { 3.5f, 4f, 3f, 3f, 3.5f };

        public static MudPatchModifier Create(LotGeometry g)
        {
            var bays = g.Spots[g.Spots.Length - 1].xMax;
            var last = g.WashBays[2];
            var patches = new List<MudPatch>
            {
                new MudPatch(new Vector2(bays - FirstBack, g.AisleY), Sizes[0]),
                new MudPatch(new Vector2(bays - SecondBack, g.AisleY), Sizes[1]),
                new MudPatch(new Vector2(g.WashBays[0].center.x, g.AisleY + AisleSide), Sizes[2]),
                new MudPatch(new Vector2(g.WashBays[1].center.x, g.AisleY - AisleSide), Sizes[3]),
                new MudPatch(new Vector2(last.xMin - WestOfLastWashBay, last.center.y), Sizes[4]),
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
