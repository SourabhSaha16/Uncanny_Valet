using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// Where people walk in a hand-made lot: the doors, the patron wait points at the curb, the ambient spawn points and a graph of walk nodes
    /// between them. A walk starts at a point, goes to the nearest walk node, follows the graph (shortest way) and ends at the target. No
    /// allocation after the constructor (the caller's list may grow).
    /// </summary>
    public sealed class AuthoredPedestrianPaths : IPedestrianPaths
    {
        private readonly WalkGraph _graph;
        private readonly Vector2 _main;
        private readonly Vector2 _side;
        private readonly Vector2 _curbCentre;
        private readonly Vector2[] _wait;
        private readonly Vector2[] _spawn;

        public AuthoredPedestrianPaths(HandmadeLotData data)
        {
            _graph = new WalkGraph(data.WalkNodes, data.WalkLinks);
            _main = data.TryGetDoor(DoorKind.Main, out var main) ? main.Position : Vector2.zero;
            _side = data.TryGetDoor(DoorKind.Side, out var side) ? side.Position : _main;
            _curbCentre = data.Curbs.Count > 0 ? data.Curbs[0].Rect.Center : _main;
            _wait = Positions(data.WaitPoints);
            _spawn = Positions(data.SpawnPoints);
        }

        public Vector2 DinerDoor => _main;

        /// <summary>The side door, where ambient people vanish and appear (the main door when the lot has none).</summary>
        public Vector2 SideDoor => _side;

        public Vector2 CurbCenter => _curbCentre;

        public int CurbSlotCount => _wait.Length;

        public Vector2 CurbSlot(int slot) => _wait.Length == 0 ? _curbCentre : _wait[Mathf.Clamp(slot, 0, _wait.Length - 1)];

        public int RoadRouteCount => _spawn.Length;

        public void RoadRoute(int index, bool leaving, List<Vector2> into)
        {
            into.Clear();
            if (_spawn.Length == 0) return;
            Walk(_spawn[Mathf.Abs(index) % _spawn.Length], _side, into);
            if (leaving) into.Reverse();
        }

        public void RouteToDoor(Vector2 from, List<Vector2> into)
        {
            into.Clear();
            Walk(from, _main, into);
        }

        public void RouteToCurb(int slot, List<Vector2> into)
        {
            into.Clear();
            Walk(_main, CurbSlot(slot), into);
        }

        private void Walk(Vector2 from, Vector2 to, List<Vector2> into)
        {
            into.Add(from);
            var a = _graph.Nearest(from);
            var b = _graph.Nearest(to);
            if (_graph.FindPath(a, b))
                for (var i = 0; i < _graph.PathCount; i++) into.Add(_graph.Position(_graph.PathNode(i)));
            into.Add(to);
        }

        private static Vector2[] Positions(List<NamedPoint> points)
        {
            var result = new Vector2[points.Count];
            for (var i = 0; i < result.Length; i++) result[i] = points[i].Position;
            return result;
        }
    }
}
