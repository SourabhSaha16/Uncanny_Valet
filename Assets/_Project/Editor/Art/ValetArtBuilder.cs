using UncannyValet.Valet;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>Creates Data/Valet/ValetArtConfig.asset: skin row, outfit and hair columns 0..23 as three layers.</summary>
    internal static class ValetArtBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/Valet/ValetArtConfig.asset";
        private const int Columns = 24;
        private const int SkinRow = 0;
        private const string ModelSheet = "Characters/Model/Character Model.png";
        private const string OutfitSheet = "Characters/Outfits/Outfit1.png";
        private const string HairSheet = "Characters/Hair/Hairs.png";

        public static ValetArtConfig Build()
        {
            var config = ArtAssetUtil.LoadOrCreate<ValetArtConfig>(AssetPath);
            var so = new SerializedObject(config);
            ArtAssetUtil.SetSprites(so, "_model", Layer(ModelSheet, "Character Model_r" + SkinRow + "_c"));
            ArtAssetUtil.SetSprites(so, "_outfit", Layer(OutfitSheet, "Outfit1_r0_c"));
            ArtAssetUtil.SetSprites(so, "_hair", Layer(HairSheet, "Hairs_r0_c"));
            ArtAssetUtil.Save(so);
            return config;
        }

        private static Sprite[] Layer(string sheet, string namePrefix)
        {
            var sprites = new Sprite[Columns];
            for (var c = 0; c < Columns; c++) sprites[c] = ArtSprites.Named(sheet, namePrefix + c);
            return sprites;
        }
    }
}
