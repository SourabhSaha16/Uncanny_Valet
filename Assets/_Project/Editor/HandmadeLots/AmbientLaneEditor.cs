using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Scene handles and add/remove buttons for the points of an ambient lane.</summary>
    [CustomEditor(typeof(AmbientLane))]
    public sealed class AmbientLaneEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            RouteHandles.Buttons(serializedObject, "_points", "lane");
        }

        private void OnSceneGUI()
        {
            serializedObject.Update();
            RouteHandles.Points(serializedObject, ((Component)target).transform, "_points", new Color(0.6f, 0.6f, 0.7f));
        }
    }
}
