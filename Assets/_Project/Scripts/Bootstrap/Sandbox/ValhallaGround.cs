using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The flat parts of the Valhalla lot: asphalt, the roads with their double yellow centre lines, sidewalks, the crosswalks and the hedge walls.</summary>
    public static class ValhallaGround
    {
        private const int AsphaltOrder = -30;
        private const int SidewalkOrder = -28;
        private const int MarkOrder = -27;
        private const int HedgeOrder = 0;
        private const float OutsideMargin = 12f;
        private const float SidewalkTile = 0.4f;
        private const float HedgeTile = 0.3f;
        private const float CentreLineBand = 1f;
        private const float CentreLineTile = 0.5f;

        /// <summary>Draws everything under <paramref name="root"/>.</summary>
        public static void Build(Transform root, ValhallaArt art, LotGeometry g, DecorLayout d, LotConfig config)
        {
            var asphalt = art.Get(ValhallaPiece.AsphaltPlain);
            var sidewalk = art.Get(ValhallaPiece.SidewalkPlain);
            var span = g.RoadSpan;
            var outside = Rect.MinMaxRect(span.x - OutsideMargin, d.FarSidewalk.yMin - OutsideMargin, span.y + OutsideMargin, d.TopSidewalk.yMax + OutsideMargin);
            ValhallaDraw.Tiled(root, "Asphalt", asphalt, outside, AsphaltOrder);
            ValhallaDraw.Tiled(root, "Far Sidewalk", sidewalk, d.FarSidewalk, SidewalkOrder, SidewalkTile);
            ValhallaDraw.Tiled(root, "Top Sidewalk", sidewalk, d.TopSidewalk, SidewalkOrder, SidewalkTile);
            CentreLine(root, art, span, d.EastLane.yMin);
            CentreLine(root, art, span, d.TopLane.yMax);
            ValhallaDraw.Tiled(root, "Right Strip", sidewalk, g.Strip, SidewalkOrder, SidewalkTile);
            var left = Rect.MinMaxRect(g.Bounds.xMin, g.Bounds.yMin, g.Curb.xMin, g.Bounds.yMax);
            ValhallaDraw.Tiled(root, "Diner Sidewalk", sidewalk, left, SidewalkOrder, SidewalkTile);
            Crosswalks(root, art, new WalkwayLayout(g, config.Site.WalkwayWidth));
            Hedges(root, art.Get(ValhallaPiece.HedgeBlock), g, config.WallThickness);
        }

        private static void CentreLine(Transform root, ValhallaArt art, Vector2 span, float y)
        {
            var band = Rect.MinMaxRect(span.x, y - CentreLineBand * 0.5f, span.y, y + CentreLineBand * 0.5f);
            ValhallaDraw.Tiled(root, "Centre Line", art.Get(ValhallaPiece.CentreLineH), band, MarkOrder, CentreLineTile);
        }

        // Crosswalks 0 and 2 run left to right (vertical bars), 1 and 3 run up and down (horizontal bars); one tile spans the band.
        private static void Crosswalks(Transform root, ValhallaArt art, WalkwayLayout walkway)
        {
            var across = art.Get(ValhallaPiece.CrosswalkV);
            var along = art.Get(ValhallaPiece.CrosswalkH);
            for (var i = 0; i < walkway.Crosswalks.Length; i++)
            {
                var area = walkway.Crosswalks[i];
                var horizontal = i % 2 == 0;
                var sprite = horizontal ? across : along;
                var scale = horizontal ? area.height / sprite.bounds.size.y : area.width / sprite.bounds.size.x;
                ValhallaDraw.Tiled(root, "Crosswalk " + i, sprite, area, MarkOrder, scale);
            }
        }

        // The four walls of LotFrame, with the entry opening in the south wall and the exit opening in the top wall.
        private static void Hedges(Transform root, Sprite hedge, LotGeometry g, float t)
        {
            var b = g.Bounds;
            var left = b.xMin - t;
            var right = b.xMax + t;
            Span(root, hedge, left, g.ExitGap.x, b.yMax + t * 0.5f, t);
            Span(root, hedge, g.ExitGap.y, right, b.yMax + t * 0.5f, t);
            Span(root, hedge, left, g.EntryGap.x, b.yMin - t * 0.5f, t);
            Span(root, hedge, g.EntryGap.y, right, b.yMin - t * 0.5f, t);
            ValhallaDraw.Tiled(root, "Hedge W", hedge, new Rect(b.xMin - t, b.yMin, t, b.height), HedgeOrder, HedgeTile);
            ValhallaDraw.Tiled(root, "Hedge E", hedge, new Rect(b.xMax, b.yMin, t, b.height), HedgeOrder, HedgeTile);
        }

        private static void Span(Transform root, Sprite hedge, float xMin, float xMax, float y, float t)
        {
            if (xMax - xMin < 0.01f) return;
            ValhallaDraw.Tiled(root, "Hedge", hedge, new Rect(xMin, y - t * 0.5f, xMax - xMin, t), HedgeOrder, HedgeTile);
        }
    }
}
