using System.Collections.Generic;
using UncannyValet.Pedestrians;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>
    /// Creates Data/Pedestrians/PedestrianConfig.asset: every skin row of the character model, every hair row of
    /// Hairs.png and the six outfits, each as rows of 24 columns (same layout the valet uses).
    /// </summary>
    internal static class PedestrianArtBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/Pedestrians/PedestrianConfig.asset";
        private const int Columns = 24;
        private const int SkinRows = 6;
        private const int HairRows = 8;
        private const int OutfitCount = 6;
        private const string ModelSheet = "Characters/Model/Character Model.png";
        private const string HairSheet = "Characters/Hair/Hairs.png";

        public static PedestrianConfig Build()
        {
            var config = ArtAssetUtil.LoadOrCreate<PedestrianConfig>(AssetPath);
            var so = new SerializedObject(config);
            ArtAssetUtil.SetSprites(so, "_model", Rows(ModelSheet, "Character Model", SkinRows));
            ArtAssetUtil.SetSprites(so, "_hair", Rows(HairSheet, "Hairs", HairRows));
            ArtAssetUtil.SetSprites(so, "_outfit", Outfits());
            ArtAssetUtil.Save(so);
            return config;
        }

        private static List<Sprite> Rows(string sheet, string baseName, int rows)
        {
            var sprites = new List<Sprite>(rows * Columns);
            for (var r = 0; r < rows; r++)
            for (var c = 0; c < Columns; c++)
                sprites.Add(ArtSprites.Named(sheet, baseName + "_r" + r + "_c" + c));
            return sprites;
        }

        private static List<Sprite> Outfits()
        {
            var sprites = new List<Sprite>(OutfitCount * Columns);
            for (var n = 1; n <= OutfitCount; n++)
            for (var c = 0; c < Columns; c++)
                sprites.Add(ArtSprites.Named("Characters/Outfits/Outfit" + n + ".png", "Outfit" + n + "_r0_c" + c));
            return sprites;
        }
    }
}
