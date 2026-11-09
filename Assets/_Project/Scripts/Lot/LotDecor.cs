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
        private WalkwayLayout _walkway;

        private WalkwayLayout Walkway
            => _walkway ?? (_walkway = new WalkwayLayout(_config, Layout, _art.Walkway, _art.CrosswalkOffset));

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

        /// <summary>
        /// Stretches <paramref name="rect"/> up and down just enough for the south sidewalk, part of the road and the
        /// foot of the diner to be in view. The rest of the scenery is cropped, so cars and people read bigger.
        /// </summary>
        public Rect ExtendView(Rect rect)
        {
            if (_config == null || _art == null) return rect;
            var layout = Layout;
            var origin = transform.position.y;
            var bottom = layout.SouthSidewalk.yMin - _art.RoadHeight * _art.ViewRoadShare + origin;
            var top = layout.BuildingBase.y + BuildingHeight() * _art.ViewBuildingShare + origin;
            return Rect.MinMaxRect(rect.xMin, Mathf.Min(rect.yMin, bottom), rect.xMax, Mathf.Max(rect.yMax, top));
        }

        /// <summary>
        /// The routes pedestrians walk, in world coordinates (open polylines, walked to the end and back). Empty
        /// until the decor has a LotConfig and a LotArtConfig.
        /// </summary>
        public Vector2[][] WalkRoutes()
        {
            if (_config == null || _art == null) return new Vector2[0][];
            var routes = Walkway.Routes;
            var world = new Vector2[routes.Length][];
            var origin = (Vector2)transform.position;
            for (var i = 0; i < routes.Length; i++)
            {
                world[i] = new Vector2[routes[i].Length];
                for (var p = 0; p < routes[i].Length; p++) world[i][p] = routes[i][p] + origin;
            }
            return world;
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
            var width = pitch * (_art.CrosswalkBars - 1) + _art.CrosswalkBarWidth;
            var zone = new Rect(curb.center.x + _art.CrosswalkOffset - width * 0.5f, l.Road.yMin, width, l.Road.height);
            LotWalkwayArt.Crossing(transform, rects, "Road Crossing", zone, _art.CrosswalkBars, _art.CrosswalkBarWidth,
                _art.CrosswalkBarGap, _art.CrosswalkColor, MarkingOrder);
            LotWalkwayArt.Build(transform, rects, Walkway, _art.Walkway, _art.CrosswalkColor);
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
