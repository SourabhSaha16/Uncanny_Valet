using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure walking routes around the diner lot, in world coordinates. Everyone crosses the road at its crosswalk, then
    /// walks north through the lot along the open strip beside the curb (over the aisle crossing) to the north
    /// sidewalk and east or west along it to the diner door. Curb standing spots sit on that strip, next to the curb zone.
    /// </summary>
    public sealed class PedestrianPathMap : IPedestrianPaths
    {
        private const int RoadStarts = 4;

        private readonly float _stripX;
        private readonly float _northY;
        private readonly float _nearY;
        private readonly float _farY;
        private readonly float _crossX;
        private readonly float _westX;
        private readonly float _eastX;
        private readonly Vector2 _curbRightCentre;
        private readonly WalkwayStyle _style;
        private readonly Vector2 _origin;

        public PedestrianPathMap(LotConfig config, DecorLayout decor, WalkwayStyle style, float roadCrossOffset, Vector2 origin)
        {
            _style = style;
            _origin = origin;
            _stripX = WalkwayLayout.StripX(config);
            _northY = decor.NorthSidewalk.center.y;
            _nearY = decor.SouthSidewalk.center.y;
            _farY = decor.FarSidewalk.center.y;
            _crossX = config.CurbPosition.x + roadCrossOffset;
            _westX = decor.SouthSidewalk.xMin - style.OffscreenReach;
            _eastX = decor.SouthSidewalk.xMax + style.OffscreenReach;
            _curbRightCentre = new Vector2(config.CurbPosition.x + config.CurbSize.x * 0.5f, config.CurbPosition.y);
            DinerDoor = World(new Vector2(decor.BuildingBase.x, _northY + style.DoorDepth));
            _doorX = decor.BuildingBase.x;
        }

        private readonly float _doorX;

        public Vector2 DinerDoor { get; }

        public int CurbSlotCount => _style.WaitSlots;

        public int RoadRouteCount => RoadStarts;

        public Vector2 CurbSlot(int slot)
        {
            var count = _style.WaitSlots;
            var index = Mathf.Clamp(slot, 0, count - 1);
            var y = _curbRightCentre.y + (index - (count - 1) * 0.5f) * _style.WaitSpacing;
            return World(new Vector2(_curbRightCentre.x + _style.WaitInset, y));
        }

        public void RoadRoute(int index, bool leaving, List<Vector2> into)
        {
            into.Clear();
            switch (Mathf.Abs(index) % RoadStarts)
            {
                case 0: AddCrossing(into, _westX); break;
                case 1: AddCrossing(into, _eastX); break;
                case 2: Add(into, _westX, _nearY); break;
                default: Add(into, _eastX, _nearY); break;
            }
            Add(into, _stripX, _nearY);
            AddNorthTail(into);
            if (leaving) into.Reverse();
        }

        public void RouteToDoor(Vector2 from, List<Vector2> into)
        {
            into.Clear();
            var local = from - _origin;
            Add(into, local.x, local.y);
            Add(into, _stripX, local.y);
            AddNorthTail(into);
        }

        public void RouteToCurb(int slot, List<Vector2> into)
        {
            into.Clear();
            var target = CurbSlot(slot) - _origin;
            Add(into, _doorX, _northY + _style.DoorDepth);
            Add(into, _doorX, _northY);
            Add(into, _stripX, _northY);
            Add(into, _stripX, target.y);
            Add(into, target.x, target.y);
        }

        // Far-side sidewalk, across the road at the crosswalk, then onto the near sidewalk.
        private void AddCrossing(List<Vector2> into, float startX)
        {
            Add(into, startX, _farY);
            Add(into, _crossX, _farY);
            Add(into, _crossX, _nearY);
        }

        private void AddNorthTail(List<Vector2> into)
        {
            Add(into, _stripX, _northY);
            Add(into, _doorX, _northY);
            Add(into, _doorX, _northY + _style.DoorDepth);
        }

        private void Add(List<Vector2> into, float x, float y) => into.Add(World(new Vector2(x, y)));

        private Vector2 World(Vector2 local) => local + _origin;
    }
}
