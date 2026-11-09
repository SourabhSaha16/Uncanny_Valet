using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Builds the Grandma's Diner scenery once at Awake from sprites: grass, a sidewalk, a road with markings and
    /// a crosswalk, the diner and trees. Lives next to the lot (not under it) and sits at the same origin.
    /// Purely visual: it has no colliders and never touches the lot rules.
    /// </summary>
    public sealed class LotDecor : MonoBehaviour
    {
        private const int GrassOrder = -30;
        private const int GroundOrder = -25;
        private const int MarkingOrder = -24;
        private const int PropOrder = 1;

        [SerializeField] private LotConfig _config;
        [SerializeField] private LotArtConfig _art;

        private DecorLayout _layout;
        private bool _hasLayout;

        private DecorLayout Layout
        {
            get
            {
                if (_hasLayout) return _layout;
                var lot = new LotGeometry(_config).Bounds;
                _layout = new DecorLayout(lot, _config.WallThickness, _art.SidewalkDepth, _art.RoadHeight,
                    _art.SideReach, _art.GrassReach);
                _hasLayout = true;
                return _layout;
            }
        }

        /// <summary>Stretches <paramref name="rect"/> up and down so the road, far sidewalk and diner are in view.</summary>
        public Rect ExtendView(Rect rect)
        {
            if (_config == null || _art == null) return rect;
            var layout = Layout;
            var origin = transform.position.y;
            var top = layout.BuildingBase.y + BuildingHeight() + origin;
            return Rect.MinMaxRect(rect.xMin, Mathf.Min(rect.yMin, layout.FarSidewalk.yMin + origin),
                rect.xMax, Mathf.Max(rect.yMax, top));
        }

        private void Awake()
        {
            if (_config == null || _art == null)
            {
                Debug.LogError("LotDecor on " + name + " needs a LotConfig and a LotArtConfig; rebuild with Setup/Build Everything.", this);
                return;
            }
            var rects = new RectFactory();
            var l = Layout;
            rects.Make(transform, "Grass", l.Grass.center, l.Grass.size, _art.GrassColor, GrassOrder);
            rects.Make(transform, "Road", l.Road.center, l.Road.size, _art.AsphaltColor, GroundOrder);
            BuildRoad(rects, l);
            BuildProps(l);
        }

        private void BuildRoad(RectFactory rects, DecorLayout l)
        {
            var dash = new Rect(l.Road.xMin, l.Road.center.y - _art.DashTileShift - 0.5f, l.Road.width, 1f);
            SpriteTiler.Fill(transform, "Road Dashes", _art.RoadDashTile, dash, MarkingOrder);
            SpriteTiler.Fill(transform, "South Sidewalk", _art.SidewalkTile, l.SouthSidewalk, GroundOrder);
            SpriteTiler.Fill(transform, "Far Sidewalk", _art.SidewalkTile, l.FarSidewalk, GroundOrder);
            SpriteTiler.Fill(transform, "North Sidewalk", _art.SidewalkTile, l.NorthSidewalk, GroundOrder);
            BuildCrosswalk(rects, l);
        }

        private void BuildCrosswalk(RectFactory rects, DecorLayout l)
        {
            var curb = new LotGeometry(_config).Curb;
            var pitch = _art.CrosswalkBarWidth + _art.CrosswalkBarGap;
            var first = curb.center.x + _art.CrosswalkOffset - pitch * (_art.CrosswalkBars - 1) * 0.5f;
            var size = new Vector2(_art.CrosswalkBarWidth, l.Road.height - _art.CrosswalkBarGap);
            for (var i = 0; i < _art.CrosswalkBars; i++)
                rects.Make(transform, "Crosswalk Bar " + i, new Vector2(first + i * pitch, l.Road.center.y), size,
                    _art.CrosswalkColor, MarkingOrder);
        }

        private void BuildProps(DecorLayout l)
        {
            SpriteTiler.Single(transform, "Diner", _art.Building, l.BuildingBase, PropOrder);
            var trees = _art.Trees;
            var treeBases = l.TreeBases();
            for (var i = 0; trees.Length > 0 && i < treeBases.Length; i++)
            {
                var tree = trees[i % trees.Length];
                var lift = tree.rect.height / tree.pixelsPerUnit * 0.5f; // tree sprites pivot at their centre
                SpriteTiler.Single(transform, "Tree " + i, tree, treeBases[i] + Vector2.up * lift, PropOrder);
            }
            var tufts = _art.Tufts;
            var spots = l.TuftSpots(tufts.Length > 0 ? _art.TuftCount : 0);
            for (var i = 0; i < spots.Length; i++)
                SpriteTiler.Single(transform, "Tuft " + i, tufts[i % tufts.Length], spots[i], GroundOrder);
        }

        private float BuildingHeight()
        {
            var b = _art.Building;
            return b == null ? 0f : b.rect.height / b.pixelsPerUnit;
        }
    }
}
