using System.IO;
using UnityEditor;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// The authoring kit of the hand-made lots (#194): Tile assets from the square Valhalla tiles (tools/slice-valhalla-tiles.py), a Tile Palette
    /// prefab grouped by name prefix, scenery prefabs from the sliced props, and the lot authoring scene template. Safe to run again: existing assets are
    /// refreshed in place, an existing scene template is never overwritten (it may hold someone's work). Menu: Uncanny Valet/Setup/Hand-made Lot Kit;
    /// also part of Build Everything.
    /// </summary>
    public static class HandmadeTileSetup
    {
        public const string SpriteFolder = "Assets/_Project/Art/ValhallaMidgard/Tiles";
        public const string TileFolder = "Assets/_Project/Data/HandmadeLots/Tiles";
        public const string PaletteFolder = "Assets/_Project/Prefabs/HandmadeLots/Palettes";
        public const string PaletteName = "Valhalla";
        public const string PalettePath = PaletteFolder + "/" + PaletteName + ".prefab";
        public const string SceneryFolder = "Assets/_Project/Prefabs/HandmadeLots/Scenery";
        public const string TemplateScenePath = "Assets/_Project/Scenes/HandmadeLots/Template_Edit.unity";

        /// <summary>Creates or refreshes the tiles, the palette and the scenery prefabs, and creates the scene template when it is missing.</summary>
        [MenuItem("Uncanny Valet/Setup/Hand-made Lot Kit")]
        public static void CreateIfMissing()
        {
            Directory.CreateDirectory(TileFolder);
            Directory.CreateDirectory(PaletteFolder);
            Directory.CreateDirectory(SceneryFolder);
            AssetDatabase.Refresh();
            var tiles = HandmadeTileAssets.Build();
            HandmadePalette.Build(tiles);
            HandmadeSceneryPrefabs.Build();
            AssetDatabase.SaveAssets();
            HandmadeTemplateScene.CreateIfMissing();
        }
    }
}
