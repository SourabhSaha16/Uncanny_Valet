using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The road below the lot as lane polylines and turn routes, in world coordinates (lot origin + local geometry).
    /// Eastbound lane (next to the lot) runs left to right, the westbound lane right to left. Patron cars turn up into
    /// the queue column at <see cref="QueueTurnDistance"/>; cars leaving the curb come down the corridor and merge in
    /// at <see cref="CurbMergeDistance"/>. Pure maths: no Unity objects.
    /// </summary>
    public sealed class RoadLayout : IRoadLayout
    {
        private const float ArcFirstDegrees = 30f;
        private const float ArcSecondDegrees = 60f;

        private readonly LotGeometry _geometry;
        private readonly Vector2 _origin;
        private readonly float _radius;
        private readonly Vector2[] _east;
        private readonly Vector2[] _west;

        public RoadLayout(LotGeometry geometry, SiteLayout site, Vector2 origin)
        {
            _geometry = geometry;
            _origin = origin;
            _radius = site.TurnRadius;
            _east = new[] { World(geometry.RoadSpan.x, geometry.EastLaneY), World(geometry.RoadSpan.y, geometry.EastLaneY) };
            _west = new[] { World(geometry.RoadSpan.y, geometry.WestLaneY), World(geometry.RoadSpan.x, geometry.WestLaneY) };
        }

        public IReadOnlyList<Vector2> EastLane => _east;

        public IReadOnlyList<Vector2> WestLane => _west;

        public float QueueTurnDistance => _geometry.QueueSlots[0].x - _radius - _geometry.RoadSpan.x;

        public float CurbMergeDistance => _geometry.ExitX + _radius - _geometry.RoadSpan.x;

        public void QueueEntryRoute(int slot, List<Vector2> into)
        {
            into.Clear();
            var g = _geometry;
            var column = g.QueueSlots[0].x;
            var lane = g.EastLaneY;
            var centre = new Vector2(column - _radius, lane + _radius);
            into.Add(World(column - _radius, lane));
            AddArc(into, centre, ArcFirstDegrees, Vector2.down, Vector2.right);
            AddArc(into, centre, ArcSecondDegrees, Vector2.down, Vector2.right);
            into.Add(World(column, lane + _radius));
            into.Add(g.QueueSlots[Mathf.Clamp(slot, 0, g.QueueSlots.Length - 1)] + _origin);
        }

        public void CurbExitRoute(List<Vector2> into)
        {
            into.Clear();
            var g = _geometry;
            var lane = g.EastLaneY;
            var centre = new Vector2(g.ExitX + _radius, lane + _radius);
            into.Add(g.Curb.center + _origin);
            into.Add(World(g.ExitX, g.Curb.yMin));
            into.Add(World(g.ExitX, lane + _radius));
            AddArc(into, centre, ArcFirstDegrees, Vector2.left, Vector2.down);
            AddArc(into, centre, ArcSecondDegrees, Vector2.left, Vector2.down);
            into.Add(World(g.ExitX + _radius, lane));
        }

        // Point on the circle at the given angle, which sweeps from <start> (0 degrees) to <end> (90 degrees).
        private void AddArc(List<Vector2> into, Vector2 centre, float degrees, Vector2 start, Vector2 end)
        {
            var rad = degrees * Mathf.Deg2Rad;
            into.Add(centre + _radius * (start * Mathf.Cos(rad) + end * Mathf.Sin(rad)) + _origin);
        }

        private Vector2 World(float x, float y) => new Vector2(x, y) + _origin;
    }
}
