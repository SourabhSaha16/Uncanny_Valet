using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Scene-view drawing shared by the lot markers (gizmos only; nothing here runs in a build).</summary>
    internal static class MarkerGizmos
    {
        private const float FillAlpha = 0.18f;
        private const float DotRadius = 0.25f;

        /// <summary>A turned, translucent box with a wire outline centred on the marker.</summary>
        public static void Box(Transform t, Vector2 size, Color color)
        {
            var previous = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(t.position, Quaternion.Euler(0f, 0f, t.eulerAngles.z), Vector3.one);
            Gizmos.color = new Color(color.r, color.g, color.b, FillAlpha);
            Gizmos.DrawCube(Vector3.zero, new Vector3(size.x, size.y, 0.01f));
            Gizmos.color = color;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0.01f));
            Gizmos.matrix = previous;
        }

        /// <summary>A polyline through points given in the marker's local space, with a dot on the first point.</summary>
        public static void Path(Transform t, Vector2[] localPoints, Color color)
        {
            if (localPoints == null || localPoints.Length == 0) return;
            Gizmos.color = color;
            Gizmos.DrawSphere(t.TransformPoint(localPoints[0]), DotRadius);
            for (var i = 0; i + 1 < localPoints.Length; i++) Gizmos.DrawLine(t.TransformPoint(localPoints[i]), t.TransformPoint(localPoints[i + 1]));
        }

        public static void Dot(Vector3 position, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(position, DotRadius);
        }

        public static void Label(Vector3 position, string text)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.Label(position, text);
#endif
        }
    }
}
