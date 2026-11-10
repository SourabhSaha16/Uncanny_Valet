using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Builds the lot v2 scenery once at Awake: grass, the two-lane road below the lot with markings and a far sidewalk,
    /// the right strip with the bus stop, the diner with its doors, the walkway bands and the trees. Lives next to the lot
    /// (not under it) and sits at the same origin. Purely visual: it has no colliders and never touches the lot rules.
    /// </summary>
    public sealed class LotDecor : MonoBehaviour
    {
        private const int GrassOrder = -30;
        private const int GroundOrder = -25;
        private const int MarkingOrder = -24;
        private const int StripOrder = -9;
        private const int PropOrder = 1;
        private const float EdgeLineWidth = 0.12f;

        [SerializeField] private LotConfig _config;
        [SerializeField] private LotArtConfig _art;

        private LotGeometry _geometry;
        private DecorLayout _layout;
        private PedestrianPathMap _paths;

        private LotGeometry Geometry => _geometry ?? (_geometry = new LotGeometry(_config));

        private DecorLayout Layout
        {
            get
            {
                if (_layout.Grass.width > 0f) return _layout;
                _layout = new DecorLayout(Geometry, _config.WallThickness, _art.SidewalkDepth, _art.GrassReach,
                    _art.BusStopSize, _art.BusStopFromBottom);
                return _layout;
            }
        }

        /// <summary>Stretches <paramref name="rect"/> down just enough for the whole road and the far sidewalk to be in view.</summary>
        public Rect ExtendView(Rect rect)
        {
            if (_config == null || _art == null) return rect;
            var bottom = Layout.FarSidewalk.yMin + transform.position.y;
            return Rect.MinMaxRect(rect.xMin, Mathf.Min(rect.yMin, bottom), rect.xMax, rect.yMax);
        }

        /// <summary>The walking routes, doors and curb standing spots in world coordinates; null until the decor has its configs.</summary>
        public IPedestrianPaths Paths
        {
            get
            {
                if (_config == null || _art == null) return null;
                return _paths ?? (_paths = new PedestrianPathMap(Geometry, _config.Site, _art.Walkway, transform.position));
            }
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
            BuildRoad(rects, l);
            BuildStrip(rects, l);
            LotWalkwayArt.Build(transform, rects, new WalkwayLayout(Geometry, _config.Site.WalkwayWidth), _art.Walkway, _art.CrosswalkColor);
            LotDinerArt.BuildDiner(transform, rects, Geometry, _art, _config);
            BuildProps(l);
        }

        private void BuildRoad(RectFactory rects, DecorLayout l)
        {
            rects.Make(transform, "Road", l.Road.center, l.Road.size, _art.AsphaltColor, GroundOrder);
            var dash = new Rect(l.Road.xMin, l.EastLane.yMin - _art.DashTileShift - 0.5f, l.Road.width, 1f);
            SpriteTiler.Fill(transform, "Road Dashes", _art.RoadDashTile, dash, MarkingOrder);
            SpriteTiler.Fill(transform, "Far Sidewalk", _art.SidewalkTile, l.FarSidewalk, GroundOrder);
            var line = new Vector2(l.Road.width, EdgeLineWidth);
            rects.Make(transform, "Road Edge N", new Vector2(l.Road.center.x, l.Road.yMax - EdgeLineWidth * 0.5f), line, _art.CrosswalkColor, MarkingOrder);
            rects.Make(transform, "Road Edge S", new Vector2(l.Road.center.x, l.Road.yMin + EdgeLineWidth * 0.5f), line, _art.CrosswalkColor, MarkingOrder);
        }

        private void BuildStrip(RectFactory rects, DecorLayout l)
        {
            SpriteTiler.Fill(transform, "Right Strip", _art.SidewalkTile, Geometry.Strip, StripOrder);
            LotDinerArt.BuildBusStop(transform, rects, l, _art, _config);
        }

        private void BuildProps(DecorLayout l)
        {
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
    }
}
