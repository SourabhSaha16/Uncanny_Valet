using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Scene handles and add/remove buttons for the route points of the car exit road.</summary>
    [CustomEditor(typeof(CarExitRoad))]
    public sealed class CarExitRoadEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            RouteHandles.Buttons(serializedObject, "_lane", "lane");
            RouteHandles.Buttons(serializedObject, "_curbExit", "curb exit");
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();
            var t = ((Component)target).transform;
            RouteHandles.Points(serializedObject, t, "_lane", new Color(1f, 0.55f, 0.2f));
            RouteHandles.Points(serializedObject, t, "_curbExit", Color.green);
        }
    }
}
