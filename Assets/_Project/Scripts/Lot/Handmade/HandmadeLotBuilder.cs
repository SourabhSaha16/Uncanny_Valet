using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// Builds the playable parts of a validated hand-made lot under its root: the curb zone with its arrows, the numbered bays (regular ids
    /// 1..n, charging ids after them), the wash stations and the mud patches. The artist's own art is left alone. The root must not be
    /// rotated or scaled.
    /// </summary>
    public sealed class HandmadeLotBuilder
    {
        private readonly Transform _root;
        private readonly LotConfig _config;
        private readonly HandmadeLotData _data;

        public HandmadeLotBuilder(Transform root, LotConfig config, HandmadeLotData data)
        {
            _root = root;
            _config = config;
            _data = data;
        }

        public SpotOccupancy Occupancy { get; private set; }
        public LotExpander Expander { get; private set; }
        public SpotGeometry Geometry { get; } = new SpotGeometry();
        public int RegularCount { get; private set; }
        public int ChargingCount { get; private set; }
        public int WashCount { get; private set; }

        public void Build()
        {
            var rects = new RectFactory();
            MudPatchBuilder.Build(_root, Patches());
            var judge = _root.gameObject.AddComponent<PerfectRunner>();
            judge.Init(_config, BuildCurb(rects));
            BuildBays(rects, judge);
            BuildWash(rects);
        }

        private CurbZone BuildCurb(RectFactory rects)
        {
            var piece = _data.Curbs[0];
            var centre = (Vector2)_root.InverseTransformPoint(piece.Rect.Center);
            var arrow = LotCurbArrows.Build(rects, _root, _config, centre, piece.Rect.Size);
            var zone = new GameObject("CurbZone");
            zone.transform.SetParent(_root, false);
            zone.transform.localPosition = centre;
            zone.AddComponent<BoxCollider2D>();
            var curb = zone.AddComponent<CurbZone>();
            curb.Init(_config, piece.Rect.Size, arrow);
            return curb;
        }

        private void BuildBays(RectFactory rects, PerfectRunner judge)
        {
            var regular = Sorted(BayKind.Regular);
            var charging = Sorted(BayKind.Charging);
            RegularCount = regular.Count;
            ChargingCount = charging.Count;
            var ids = new List<int>();
            for (var id = 1; id <= regular.Count + charging.Count; id++) ids.Add(id);
            Occupancy = new SpotOccupancy(ids, regular.Count + 1);
            var spots = new ParkingSpot[ids.Count];
            for (var i = 0; i < spots.Length; i++)
            {
                var isCharging = i >= regular.Count;
                var rect = (isCharging ? charging[i - regular.Count] : regular[i]).Rect;
                spots[i] = SpotBuilder.Build(rects, _root, _config, i + 1, Local(rect.Center), rect.Size, rect.RotationDegrees,
                    isCharging, Occupancy, null, judge);
                Geometry.Set(i + 1, new SpotBounds(rect.Center, SpotBuilder.InnerSize(rect.Size), rect.RotationDegrees));
            }
            Expander = _root.gameObject.AddComponent<LotExpander>();
            Expander.Init(Occupancy, spots);
        }

        private void BuildWash(RectFactory rects)
        {
            var washes = Sorted(BayKind.Wash);
            WashCount = washes.Count;
            if (washes.Count == 0) return;
            var station = _root.gameObject.AddComponent<StationSwitch>();
            for (var i = 0; i < washes.Count; i++)
            {
                var rect = washes[i].Rect;
                station.Init(StationBuilder.BuildWash(rects, _root, _config, Local(rect.Center), rect.Size, rect.RotationDegrees, i + 1));
            }
        }

        private List<BayData> Sorted(BayKind kind)
        {
            var list = _data.Bays.FindAll(b => b.Kind == kind);
            list.Sort((a, b) => a.Number.CompareTo(b.Number));
            return list;
        }

        private MudPatch[] Patches()
        {
            var patches = new MudPatch[_data.Hazards.Count];
            for (var i = 0; i < patches.Length; i++)
                patches[i] = new MudPatch(Local(_data.Hazards[i].Rect.Center), _data.Hazards[i].Rect.Size.x, _data.Hazards[i].Rect.Size.y);
            return patches;
        }

        private Vector2 Local(Vector2 world) => _root.InverseTransformPoint(world);
    }
}
