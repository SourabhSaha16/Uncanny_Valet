using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Scene handles and inspector buttons for a route stored as a <c>Vector2[]</c> (or one <c>Vector2</c>) in the local space of a marker.</summary>
    public static class RouteHandles
    {
        private const float HandleSize = 0.6f;
        /// <summary>How far a new point is put from the last one, along the last segment (or along +X when the route has one point).</summary>
        private const float NewPointStep = 4f;

        /// <summary>Draws a draggable handle for every point of the array field; writes moves back with undo.</summary>
        public static void Points(SerializedObject so, Transform space, string field, Color color)
        {
            var points = so.FindProperty(field);
            Handles.color = color;
            for (var i = 0; i < points.arraySize; i++) Handle(points.GetArrayElementAtIndex(i), space, field + " " + i);
        }

        /// <summary>Draws a draggable handle for one point field.</summary>
        public static void Point(SerializedObject so, Transform space, string field, Color color)
        {
            Handles.color = color;
            Handle(so.FindProperty(field), space, field);
        }

        /// <summary>Inspector buttons that append a point to, or remove the last point of, the array field.</summary>
        public static void Buttons(SerializedObject so, string field, string label)
        {
            var points = so.FindProperty(field);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add " + label + " point")) Append(points);
                using (new EditorGUI.DisabledScope(points.arraySize == 0))
                    if (GUILayout.Button("Remove last " + label + " point")) points.arraySize--;
            }
            so.ApplyModifiedProperties();
        }

        private static void Handle(SerializedProperty point, Transform space, string label)
        {
            var world = space.TransformPoint(point.vector2Value);
            EditorGUI.BeginChangeCheck();
            var moved = Handles.FreeMoveHandle(world, HandleSize, Vector3.zero, Handles.CircleHandleCap);
            Handles.Label(world + Vector3.up * HandleSize, label);
            if (!EditorGUI.EndChangeCheck()) return;
            point.vector2Value = space.InverseTransformPoint(moved);
            point.serializedObject.ApplyModifiedProperties();
        }

        private static void Append(SerializedProperty points)
        {
            var count = points.arraySize;
            var step = new Vector2(NewPointStep, 0f);
            var last = Vector2.zero;
            if (count > 0) last = points.GetArrayElementAtIndex(count - 1).vector2Value;
            if (count > 1) step = (last - points.GetArrayElementAtIndex(count - 2).vector2Value).normalized * NewPointStep;
            points.arraySize = count + 1;
            points.GetArrayElementAtIndex(count).vector2Value = last + step;
        }
    }
}
