using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Scene handles and add/remove buttons for the route points of the car entry road.</summary>
    [CustomEditor(typeof(CarEntryRoad))]
    public sealed class CarEntryRoadEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            RouteHandles.Buttons(serializedObject, "_lane", "lane");
            RouteHandles.Buttons(serializedObject, "_bend", "bend");
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();
            var t = ((Component)target).transform;
            RouteHandles.Points(serializedObject, t, "_lane", new Color(0.3f, 0.8f, 1f));
            RouteHandles.Point(serializedObject, t, "_queueTurn", Color.green);
            RouteHandles.Points(serializedObject, t, "_bend", Color.green);
        }
    }
}
