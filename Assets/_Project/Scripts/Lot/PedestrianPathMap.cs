using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure walking routes of lot v2, in world coordinates. Ambient people appear on the right strip (the outside road and
    /// bus stop), walk along the long crossing over the drive aisle, up the north-south crosswalk and along the side-door
    /// leg to the side door. Patrons use the main door only: from their car (or the diner) via the corridor crosswalk and
    /// over the curb. Curb standing spots sit in the open gap between the diner wall and the curb zone, so patrons face east to the zone.
    /// </summary>
    public sealed class PedestrianPathMap : IPedestrianPaths
    {
        // Where along the strip's height each ambient route starts (0 = bottom wall, 1 = top wall).
        private static readonly float[] StartShares = { 0.9f, 0.7f, 0.3f, 0.1f };
        private const float BayAreaMargin = 0.5f;

        private readonly LotGeometry _g;
        private readonly WalkwayStyle _style;
        private readonly Vector2 _origin;
        private readonly Vector2 _mainDoor;
        private readonly Vector2 _sideDoor;

        public PedestrianPathMap(LotGeometry geometry, SiteLayout site, WalkwayStyle style, Vector2 origin)
        {
            _g = geometry;
            _style = style;
            _origin = origin;
            var inside = geometry.Diner.xMax - site.DoorDepth;
            _mainDoor = new Vector2(inside, geometry.MainDoor.center.y);
            _sideDoor = new Vector2(inside, geometry.SideDoor.center.y);
            DinerDoor = _mainDoor + origin;
        }

        /// <summary>The main door (patrons); ambient people use the side door at the end of the road routes.</summary>
        public Vector2 DinerDoor { get; }

        /// <summary>The side door in world coordinates: where ambient people vanish and appear.</summary>
        public Vector2 SideDoor => _sideDoor + _origin;

        public Vector2 CurbCenter => _g.Curb.center + _origin;

        public int CurbSlotCount => _style.WaitSlots;

        public int RoadRouteCount => StartShares.Length;

        public Vector2 CurbSlot(int slot)
        {
            var count = _style.WaitSlots;
            var index = Mathf.Clamp(slot, 0, count - 1);
            var y = _g.Curb.center.y + (index - (count - 1) * 0.5f) * _style.WaitSpacing;
            return new Vector2(_g.CurbGap.center.x, y) + _origin;
        }

        public void RoadRoute(int index, bool leaving, List<Vector2> into)
        {
            into.Clear();
            var share = StartShares[Mathf.Abs(index) % StartShares.Length];
            var stripX = _g.Strip.center.x;
            Add(into, stripX, _g.Strip.yMin + _g.Strip.height * share);
            Add(into, stripX, _g.AisleY);
            Add(into, _g.CrossingX, _g.AisleY);
            Add(into, _g.CrossingX, _sideDoor.y);
            Add(into, _sideDoor.x, _sideDoor.y);
            if (leaving) into.Reverse();
        }

        public void RouteToDoor(Vector2 from, List<Vector2> into)
        {
            into.Clear();
            var local = from - _origin;
            Add(into, local.x, local.y);
            if (local.x > _g.CrossingX + BayAreaMargin)
            {
                Add(into, local.x, _g.AisleY); // out of the bay onto the aisle first, never across other bays
                Add(into, _g.CrossingX, _g.AisleY);
            }
            else Add(into, _g.CrossingX, local.y);
            Add(into, _g.CrossingX, _mainDoor.y);
            Add(into, _mainDoor.x, _mainDoor.y);
        }

        public void RouteToCurb(int slot, List<Vector2> into)
        {
            into.Clear();
            into.Add(_mainDoor + _origin);
            into.Add(CurbSlot(slot));
        }

        private void Add(List<Vector2> into, float x, float y) => into.Add(new Vector2(x, y) + _origin);
    }
}
