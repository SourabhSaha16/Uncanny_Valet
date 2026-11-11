using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// The two one-way roads as lane polylines and turn routes, in world coordinates (lot origin + local geometry).
    /// Both lanes run left to right: the entry lane below the lot and the exit lane above its top wall. Patron cars turn
    /// up into the queue column at <see cref="QueueTurnDistance"/>; delivered cars leave the curb straight up through the
    /// top wall gap, turn right and merge into the exit lane at <see cref="CurbMergeDistance"/>. Pure maths: no Unity objects.
    /// </summary>
    public sealed class RoadLayout : IRoadLayout
    {
        private const float ArcFirstDegrees = 30f;
        private const float ArcSecondDegrees = 60f;

        private readonly LotGeometry _geometry;
        private readonly Vector2 _origin;
        private readonly float _radius;
        private readonly Vector2[] _east;
        private readonly Vector2[] _top;

        public RoadLayout(LotGeometry geometry, SiteLayout site, Vector2 origin)
        {
            _geometry = geometry;
            _origin = origin;
            _radius = site.TurnRadius;
            _east = new[] { World(geometry.RoadSpan.x, geometry.EastLaneY), World(geometry.RoadSpan.y, geometry.EastLaneY) };
            _top = new[] { World(geometry.RoadSpan.x, geometry.TopLaneY), World(geometry.RoadSpan.y, geometry.TopLaneY) };
        }

        public IReadOnlyList<Vector2> EastLane => _east;

        public IReadOnlyList<Vector2> TopLane => _top;

        public float QueueTurnDistance => _geometry.QueueSlots[0].x - _radius - _geometry.RoadSpan.x;

        public float CurbMergeDistance => _geometry.Curb.center.x + _radius - _geometry.RoadSpan.x;

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
            var x = g.Curb.center.x;
            var lane = g.TopLaneY;
            var centre = new Vector2(x + _radius, lane - _radius);
            into.Add(g.Curb.center + _origin);
            into.Add(World(x, g.Curb.yMax));
            into.Add(World(x, lane - _radius));
            AddArc(into, centre, ArcFirstDegrees, Vector2.left, Vector2.up);
            AddArc(into, centre, ArcSecondDegrees, Vector2.left, Vector2.up);
            into.Add(World(x + _radius, lane));
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
