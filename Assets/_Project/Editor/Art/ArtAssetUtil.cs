using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>Small helpers for creating and filling the art config assets.</summary>
    internal static class ArtAssetUtil
    {
        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        public static void SetObject(SerializedObject so, string field, Object value)
        {
            so.FindProperty(field).objectReferenceValue = value;
        }

        public static void SetColor(SerializedObject so, string field, Color value)
        {
            so.FindProperty(field).colorValue = value;
        }

        /// <summary>Stores the non-null sprites in an array field.</summary>
        public static void SetSprites(SerializedObject so, string field, IList<Sprite> sprites)
        {
            var kept = new List<Sprite>();
            foreach (var sprite in sprites) if (sprite != null) kept.Add(sprite);
            var array = so.FindProperty(field);
            array.arraySize = kept.Count;
            for (var i = 0; i < kept.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = kept[i];
        }

        public static void Save(SerializedObject so)
        {
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(so.targetObject);
        }
    }
}
