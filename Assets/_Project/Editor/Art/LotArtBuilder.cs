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
        private const string DinerBuilding = "Environment/Stations/Building.png";

        // Pixel positions (x, y from the top-left) found by inspecting the sheets; see docs/changelogs/pivot-visuals.md.
        private static readonly Vector2Int CleanCobble = new Vector2Int(264, 40);
        private static readonly Vector2Int PlainAsphalt = new Vector2Int(16, 80);
        private static readonly Vector2Int DinerPoint = new Vector2Int(88, 160); // inside the tall building sprite

        public static LotArtConfig Build()
        {
            var config = ArtAssetUtil.LoadOrCreate<LotArtConfig>(AssetPath);
            var so = new SerializedObject(config);
            ArtAssetUtil.SetObject(so, "_sidewalkTile", ArtSprites.AtPoint(Tiles, CleanCobble.x, CleanCobble.y));
            ArtAssetUtil.SetObject(so, "_roadDashTile", ArtSprites.Named(Road, "Road_r6_c0"));
            ArtAssetUtil.SetObject(so, "_dinerBuilding", ArtSprites.AtPoint(DinerBuilding, DinerPoint.x, DinerPoint.y));
            ArtAssetUtil.SetColor(so, "_asphaltColor", ArtSprites.PixelColor(Road, PlainAsphalt.x, PlainAsphalt.y));
            ArtAssetUtil.Save(so);
            return config;
        }
    }
}
