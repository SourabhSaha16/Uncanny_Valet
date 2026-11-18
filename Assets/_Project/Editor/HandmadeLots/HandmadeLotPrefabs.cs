using System.Collections.Generic;
using System.IO;
using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// Where hand-made lots are kept (one prefab per lot, <c>Lot_Name.prefab</c>) and the menu that saves the lot being edited there. The lot root
    /// is the object with the HandmadeLot component; its tilemaps, markers and scenery must be its children.
    /// </summary>
    public static class HandmadeLotPrefabs
    {
        public const string Folder = "Assets/_Project/Prefabs/HandmadeLots";
        public const string NamePrefix = "Lot_";

        /// <summary>The asset path a lot called <paramref name="lotName"/> is saved to (the <c>Lot_</c> prefix is added once, bad file characters become underscores).</summary>
        public static string PathFor(string lotName)
        {
            var name = lotName.StartsWith(NamePrefix) ? lotName.Substring(NamePrefix.Length) : lotName;
            foreach (var bad in Path.GetInvalidFileNameChars()) name = name.Replace(bad, '_');
            return Folder + "/" + NamePrefix + name.Trim() + ".prefab";
        }

        /// <summary>The paths of the finished lot prefabs (the <c>Lot_*.prefab</c> files directly in <see cref="Folder"/>).</summary>
        public static List<string> AllLotPaths()
        {
            var paths = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab " + NamePrefix, new[] { Folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetDirectoryName(path).Replace('\\', '/') == Folder && Path.GetFileName(path).StartsWith(NamePrefix)) paths.Add(path);
            }
            return paths;
        }

        /// <summary>Saves the lot of the open scene or prefab stage; refuses while the validator finds an error. Returns the asset path, or null.</summary>
        [MenuItem("Uncanny Valet/Lot/Save lot prefab")]
        public static string SaveOpenLot()
        {
            var lots = LotValidation.FindLots();
            if (lots.Count != 1)
            {
                Debug.LogError("Save lot prefab needs exactly one HandmadeLot in the open scene or prefab; found " + lots.Count + ".");
                return null;
            }
            var issues = LotValidation.Check(lots[0]);
            if (HandmadeLotRules.HasErrors(issues))
            {
                Debug.LogError("Hand-made lot " + lots[0].name + " not saved: " + LotValidation.ErrorSummary(issues), lots[0]);
                return null;
            }
            Directory.CreateDirectory(Folder);
            var path = PathFor(lots[0].name);
            PrefabUtility.SaveAsPrefabAsset(lots[0].gameObject, path);
            Debug.Log("Saved hand-made lot to " + path);
            return path;
        }
    }
}
