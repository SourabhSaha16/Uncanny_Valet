using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Import
{
    /// <summary>
    /// Import settings of the Valhalla Drive sprites (Assets/_Project/Art/ValhallaMidgard, cut by tools/slice-valhalla.py): one sprite per PNG,
    /// point filter, no compression. The lot pieces, Thor and the pedestrians come at different source scales, so each folder has its own
    /// pixels per unit; the characters pivot a little above their feet so the body sits on the physics centre.
    /// </summary>
    public static class ValhallaImport
    {
        public const string Root = ArtPostprocessor.ArtRoot + "ValhallaMidgard/";
        /// <summary>Lot pieces: a bay (95 source pixels tall) comes out at the 4.4 unit height of a parking bay.</summary>
        public const int LotPixelsPerUnit = 22;
        /// <summary>Thor: 228 source pixels tall, about 1.3 units before the valet's scale.</summary>
        public const int ThorPixelsPerUnit = 175;
        /// <summary>Pedestrians: 106 source pixels tall, about 1.2 units before the pedestrian scale.</summary>
        public const int NpcPixelsPerUnit = 90;
        private const float CharacterPivotY = 0.3f;

        /// <summary>True when the asset belongs to the Valhalla folder.</summary>
        public static bool Handles(string assetPath) => assetPath.StartsWith(Root) && assetPath.EndsWith(".png");

        /// <summary>Applies the settings for the folder the asset is in.</summary>
        public static void Apply(TextureImporter importer, string assetPath)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = PixelsPerUnit(assetPath);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = IsCharacter(assetPath) ? new Vector2(0.5f, CharacterPivotY) : new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);
        }

        private static bool IsCharacter(string path) => path.StartsWith(Root + "Thor/") || path.StartsWith(Root + "Npc/");

        private static int PixelsPerUnit(string path)
        {
            if (path.StartsWith(Root + "Thor/")) return ThorPixelsPerUnit;
            return path.StartsWith(Root + "Npc/") ? NpcPixelsPerUnit : LotPixelsPerUnit;
        }
    }
}
