using UncannyValet.Lot;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>Creates Data/Lot/LotArtConfig.asset and fills it with the Grandma's Diner scenery sprites.</summary>
    internal static class LotArtBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/Lot/LotArtConfig.asset";
        private const string Tiles = "Environment/Tiles/Tiles.png";
        private const string Road = "Environment/Road/Road.png";
        private const string Trees = "Environment/Trees/Tree-Sheet.png";
        private const string DinerBuilding = "Environment/Stations/Building.png";
        private const int TreeCount = 4;
        private static readonly Vector2Int DinerPoint = new Vector2Int(88, 160); // inside the tall building sprite

        // Pixel positions (x, y from the top-left) found by inspecting the sheets; see docs/changelogs/pivot-visuals.md.
        private static readonly Vector2Int CleanCobble = new Vector2Int(264, 40);
        private static readonly Vector2Int GrassPatch = new Vector2Int(40, 92);
        private static readonly Vector2Int PlainAsphalt = new Vector2Int(16, 80);
        private static readonly Vector2Int[] TuftPoints =
        {
            new Vector2Int(8, 118), new Vector2Int(6, 152), new Vector2Int(26, 153),
            new Vector2Int(21, 167), new Vector2Int(10, 183), new Vector2Int(24, 183)
        };

        public static LotArtConfig Build()
        {
            var config = ArtAssetUtil.LoadOrCreate<LotArtConfig>(AssetPath);
            var so = new SerializedObject(config);
            ArtAssetUtil.SetObject(so, "_sidewalkTile", ArtSprites.AtPoint(Tiles, CleanCobble.x, CleanCobble.y));
            ArtAssetUtil.SetObject(so, "_roadDashTile", ArtSprites.Named(Road, "Road_r6_c0"));
            ArtAssetUtil.SetObject(so, "_dinerBuilding", ArtSprites.AtPoint(DinerBuilding, DinerPoint.x, DinerPoint.y));
            ArtAssetUtil.SetSprites(so, "_trees", TreeSprites());
            ArtAssetUtil.SetSprites(so, "_tufts", TuftSprites());
            ArtAssetUtil.SetColor(so, "_grassColor", ArtSprites.PixelColor(Tiles, GrassPatch.x, GrassPatch.y));
            ArtAssetUtil.SetColor(so, "_asphaltColor", ArtSprites.PixelColor(Road, PlainAsphalt.x, PlainAsphalt.y));
            ArtAssetUtil.Save(so);
            return config;
        }

        private static Sprite[] TreeSprites()
        {
            var trees = new Sprite[TreeCount];
            for (var i = 0; i < TreeCount; i++) trees[i] = ArtSprites.Named(Trees, "Tree-Sheet_r0_c" + i);
            return trees;
        }

        private static Sprite[] TuftSprites()
        {
            var tufts = new Sprite[TuftPoints.Length];
            for (var i = 0; i < tufts.Length; i++) tufts[i] = ArtSprites.AtPoint(Tiles, TuftPoints[i].x, TuftPoints[i].y);
            return tufts;
        }
    }
}
