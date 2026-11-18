using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Editor helpers that place a marker prefab and set its serialized fields, for the generators of hand-made lots.</summary>
    internal static class MarkerKit
    {
        /// <summary>Instantiates the marker prefab <paramref name="prefab"/> under <paramref name="parent"/> at a local position, with a new name.</summary>
        public static GameObject Place(Transform parent, string prefab, string name, Vector2 localPosition)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(HandmadeMarkerPrefabs.PathOf(prefab));
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            go.name = name;
            go.transform.localPosition = localPosition;
            return go;
        }

        /// <summary>Sets an int field on the first marker component of the object (the one that is not a Transform or collider).</summary>
        public static void SetInt(GameObject go, string field, int value) => Edit(go, field, p => p.intValue = value);

        public static void SetPoint(GameObject go, string field, Vector2 value) => Edit(go, field, p => p.vector2Value = value);

        public static void SetReference(GameObject go, string field, Object value) => Edit(go, field, p => p.objectReferenceValue = value);

        /// <summary>Replaces a Vector2 array field.</summary>
        public static void SetPoints(GameObject go, string field, params Vector2[] points)
        {
            Edit(go, field, p =>
            {
                p.arraySize = points.Length;
                for (var i = 0; i < points.Length; i++) p.GetArrayElementAtIndex(i).vector2Value = points[i];
            });
        }

        /// <summary>Sizes the BoxCollider2D of a solid block.</summary>
        public static void SetBox(GameObject go, Vector2 size)
        {
            var so = new SerializedObject(go.GetComponent<BoxCollider2D>());
            so.FindProperty("m_Size").vector2Value = size;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Edit(GameObject go, string field, System.Action<SerializedProperty> set)
        {
            foreach (var component in go.GetComponents<MonoBehaviour>())
            {
                var so = new SerializedObject(component);
                var property = so.FindProperty(field);
                if (property == null) continue;
                set(property);
                so.ApplyModifiedPropertiesWithoutUndo();
                return;
            }
            Debug.LogError("No component of " + go.name + " has the field " + field);
        }
    }
}
