using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// The two roads of a hand-made lot, from the authored entry and exit road markers. The routes (with their corners rounded) are built once in the
    /// constructor, so asking for one only copies points. Lanes are world-space polylines in travel order.
    /// </summary>
    public sealed class AuthoredRoadLayout : IRoadLayout
    {
        private const int StepsPerCorner = 6;

        private readonly Vector2[] _entry;
        private readonly Vector2[] _exit;
        private readonly Vector2[] _entryBack;
        private readonly Vector2[] _exitBack;
        private readonly Vector2[][] _queueRoutes;
        private readonly Vector2[] _curbExit;

        public AuthoredRoadLayout(HandmadeLotData data)
        {
            var scratch = new List<Vector2>();
            var radius = data.Requirements.TurnRadius;
            _entry = Array.Empty<Vector2>();
            _exit = Array.Empty<Vector2>();
            _curbExit = Array.Empty<Vector2>();
            ArrivalSlots = data.SlotCentres();
            _queueRoutes = new Vector2[ArrivalSlots.Length][];
            if (data.EntryRoads.Count > 0)
            {
                var road = data.EntryRoads[0];
                _entry = road.Lane;
                QueueTurnDistance = RouteMath.DistanceAlong(road.QueueTurn, road.Lane);
                for (var slot = 0; slot < _queueRoutes.Length; slot++) _queueRoutes[slot] = BuildQueueRoute(road, slot, radius, scratch);
            }
            if (data.ExitRoads.Count > 0)
            {
                var road = data.ExitRoads[0];
                _exit = road.Lane;
                RouteRounder.Round(road.CurbExit, radius, StepsPerCorner, scratch);
                _curbExit = scratch.ToArray();
                if (road.CurbExit.Length > 0) CurbMergeDistance = RouteMath.DistanceAlong(road.CurbExit[road.CurbExit.Length - 1], road.Lane);
            }
            _entryBack = data.EntryBackLanes.Count > 0 ? data.EntryBackLanes[0].Points : Array.Empty<Vector2>();
            _exitBack = data.ExitBackLanes.Count > 0 ? data.ExitBackLanes[0].Points : Array.Empty<Vector2>();
        }

        /// <summary>The queue slot centres, slot 0 (the valet pickup nearest the curb) first.</summary>
        public Vector2[] ArrivalSlots { get; }

        public IReadOnlyList<Vector2> EastLane => _entry;

        public IReadOnlyList<Vector2> TopLane => _exit;

        public IReadOnlyList<Vector2> EastBackLane => _entryBack;

        public IReadOnlyList<Vector2> TopBackLane => _exitBack;

        public float QueueTurnDistance { get; }

        public float CurbMergeDistance { get; }

        public void QueueEntryRoute(int slot, List<Vector2> into)
        {
            into.Clear();
            if (_queueRoutes.Length == 0) return;
            into.AddRange(_queueRoutes[Mathf.Clamp(slot, 0, _queueRoutes.Length - 1)]);
        }

        public void CurbExitRoute(List<Vector2> into)
        {
            into.Clear();
            into.AddRange(_curbExit);
        }

        private Vector2[] BuildQueueRoute(EntryRoadData road, int slot, float radius, List<Vector2> scratch)
        {
            var raw = new List<Vector2>(road.Bend.Length + 2) { road.QueueTurn };
            raw.AddRange(road.Bend);
            raw.Add(ArrivalSlots[slot]);
            RouteRounder.Round(raw, radius, StepsPerCorner, scratch);
            return scratch.ToArray();
        }
    }
}
