using System;
using UncannyValet.Bootstrap.Sandbox;
using UncannyValet.EditorTools.Art;
using UncannyValet.EditorTools.Import;
using UncannyValet.Pedestrians;
using UncannyValet.Valet;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Builds the data assets of the Valhalla art for the sandbox from the sliced sprites (tools/slice-valhalla.py): the lot sprite list, Thor's walk cycle as a valet
    /// art config and the eight pedestrian looks as a pedestrian config. The walk sheets have four frames per direction in the order down, left, right, up, in one
    /// full-colour sprite per frame; the outfit and hair layers stay empty.
    /// </summary>
    public static class ValhallaAssetSetup
    {
        public const string ArtPath = "Assets/_Project/Data/Sandbox/ValhallaArt.asset";
        public const string ThorPath = "Assets/_Project/Data/Sandbox/ValhallaThorArt.asset";
        public const string CrowdPath = "Assets/_Project/Data/Sandbox/ValhallaPedestrianConfig.asset";

        private const int Frames = 4;
        private const int Columns = Frames * 4;
        private const float ThorFramesPerSecond = 9f;
        private const float WalkerFramesPerSecond = 8f;
        private static readonly string[] Directions = { "down", "left", "right", "up" };
        private static readonly string[] Looks = { "grandma", "chef", "builder", "brown_man", "orange_girl", "black_girl", "blue_cap_man", "flat_cap_man" };

        /// <summary>Creates or refreshes the lot art list.</summary>
        public static ValhallaArt LotArt()
        {
            var art = ArtAssetUtil.LoadOrCreate<ValhallaArt>(ArtPath);
            var pieces = (ValhallaPiece[])Enum.GetValues(typeof(ValhallaPiece));
            var so = new SerializedObject(art);
            var list = so.FindProperty("_sprites");
            list.arraySize = pieces.Length;
            for (var i = 0; i < pieces.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = Load("Lot/" + FileName(pieces[i]));
            ArtAssetUtil.Save(so);
            return art;
        }

        /// <summary>Creates or refreshes Thor's valet art config.</summary>
        public static ValetArtConfig Thor()
        {
            var config = ArtAssetUtil.LoadOrCreate<ValetArtConfig>(ThorPath);
            var so = new SerializedObject(config);
            var frames = new Sprite[Columns];
            for (var d = 0; d < Directions.Length; d++)
            for (var f = 0; f < Frames; f++) frames[d * Frames + f] = Load("Thor/thor_" + Directions[d] + "_" + f);
            Fill(so.FindProperty("_model"), frames);
            Fill(so.FindProperty("_outfit"), new Sprite[Columns]);
            Fill(so.FindProperty("_hair"), new Sprite[Columns]);
            so.FindProperty("_framesPerGroup").intValue = Frames;
            so.FindProperty("_framesPerSecond").floatValue = ThorFramesPerSecond;
            so.FindProperty("_downStart").intValue = 0;
            so.FindProperty("_leftStart").intValue = Frames;
            so.FindProperty("_rightStart").intValue = Frames * 2;
            so.FindProperty("_upStart").intValue = Frames * 3;
            ArtAssetUtil.Save(so);
            return config;
        }

        /// <summary>Creates (as a copy of the game's) or refreshes the pedestrian config with the eight Valhalla looks.</summary>
        public static PedestrianConfig Crowd()
        {
            if (AssetDatabase.LoadAssetAtPath<PedestrianConfig>(CrowdPath) == null)
                AssetDatabase.CopyAsset(MidgardWorldSetup.PedestrianConfigPath, CrowdPath);
            var config = AssetDatabase.LoadAssetAtPath<PedestrianConfig>(CrowdPath);
            var so = new SerializedObject(config);
            var model = new Sprite[Looks.Length * Columns];
            for (var l = 0; l < Looks.Length; l++)
            for (var d = 0; d < Directions.Length; d++)
            for (var f = 0; f < Frames; f++) model[l * Columns + d * Frames + f] = Load("Npc/npc_" + Looks[l] + "_" + Directions[d] + "_" + f);
            Fill(so.FindProperty("_model"), model);
            Fill(so.FindProperty("_outfit"), new Sprite[Columns]);
            Fill(so.FindProperty("_hair"), new Sprite[Columns]);
            so.FindProperty("_columns").intValue = Columns;
            so.FindProperty("_framesPerGroup").intValue = Frames;
            so.FindProperty("_framesPerSecond").floatValue = WalkerFramesPerSecond;
            so.FindProperty("_downStart").intValue = 0;
            so.FindProperty("_leftStart").intValue = Frames;
            so.FindProperty("_rightStart").intValue = Frames * 2;
            so.FindProperty("_upStart").intValue = Frames * 3;
            so.FindProperty("_flipRight").boolValue = false;
            so.FindProperty("_flipLeft").boolValue = false;
            ArtAssetUtil.Save(so);
            return config;
        }

        /// <summary>The file name of a piece: PascalCase to snake case, a number counting as its own word (Bay1 is bay_1).</summary>
        public static string FileName(ValhallaPiece piece)
        {
            var name = piece.ToString();
            var text = new System.Text.StringBuilder();
            for (var i = 0; i < name.Length; i++)
            {
                var starts = i > 0 && (char.IsUpper(name[i]) || (char.IsDigit(name[i]) && !char.IsDigit(name[i - 1])));
                if (starts) text.Append('_');
                text.Append(char.ToLowerInvariant(name[i]));
            }
            return text.ToString();
        }

        private static void Fill(SerializedProperty list, Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static Sprite Load(string relative)
        {
            var path = ValhallaImport.Root + relative + ".png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) Debug.LogError("Valhalla sprite missing: " + path + ". Run tools/slice-valhalla.py and re-import.");
            return sprite;
        }
    }
}
