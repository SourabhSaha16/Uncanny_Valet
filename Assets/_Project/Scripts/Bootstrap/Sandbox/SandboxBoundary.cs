using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The solid ring around a sandbox lot: it closes the entry and exit openings of the walls, which lead nowhere here.</summary>
    public static class SandboxBoundary
    {
        /// <summary>The four rectangles (bottom, top, left, right) that enclose <paramref name="area"/> from the outside, each <paramref name="thickness"/> thick; the corners are covered.</summary>
        public static Rect[] Segments(Rect area, float thickness)
        {
            var wide = area.width + thickness * 2f;
            return new[]
            {
                new Rect(area.xMin - thickness, area.yMin - thickness, wide, thickness),
                new Rect(area.xMin - thickness, area.yMax, wide, thickness),
                new Rect(area.xMin - thickness, area.yMin, thickness, area.height),
                new Rect(area.xMax, area.yMin, thickness, area.height),
            };
        }

        /// <summary>Adds one box collider per segment under <paramref name="parent"/> (a child object each).</summary>
        public static void Build(Transform parent, Rect area, float thickness)
        {
            var segments = Segments(area, thickness);
            var group = new GameObject("Sandbox Boundary").transform;
            group.SetParent(parent, false);
            for (var i = 0; i < segments.Length; i++)
            {
                var go = new GameObject("Boundary " + i);
                go.transform.SetParent(group, false);
                go.transform.position = segments[i].center;
                go.AddComponent<BoxCollider2D>().size = segments[i].size;
            }
        }
    }
}
