using UncannyValet.Cars;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>Creates Data/Cars/CarArtConfig.asset: the top-down cells of Cars-Sheet plus their measured sizes.</summary>
    internal static class CarArtBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/Cars/CarArtConfig.asset";
        private const string Sheet = "Cars/Cars-Sheet.png";
        private const string SheetName = "Cars-Sheet";

        // (row, column) of every top-down cell that fits the 1 x 2 car footprint: 12 civilian cars and the 2 police
        // cars (same footprint, so spot fitting is unchanged). Ambulances (r0c2, r2c6) and the fire truck are left out:
        // they are longer and would not fit. A car id also mirrors the sprite for half of the ids (CarArtMath.IsFlipped).
        private static readonly Vector2Int[] Regular =
        {
            new Vector2Int(0, 0), new Vector2Int(0, 1), new Vector2Int(0, 5), new Vector2Int(0, 6),
            new Vector2Int(1, 1), new Vector2Int(1, 5), new Vector2Int(2, 0), new Vector2Int(2, 1),
            new Vector2Int(2, 4), new Vector2Int(2, 5), new Vector2Int(3, 0), new Vector2Int(3, 5),
            new Vector2Int(1, 6), new Vector2Int(3, 1)
        };

        // Cyan-glow cells reserved for EVs.
        private static readonly Vector2Int[] Evs = { new Vector2Int(1, 0), new Vector2Int(3, 4) };

        /// <summary>Rotation added to every sprite so its nose points +Y. The sheet appears to face up; verify in Unity.</summary>
        public const float RotationOffset = 0f;

        public static CarArtConfig Build()
        {
            var config = ArtAssetUtil.LoadOrCreate<CarArtConfig>(AssetPath);
            var so = new SerializedObject(config);
            Fill(so.FindProperty("_cars"), Regular);
            Fill(so.FindProperty("_evCars"), Evs);
            ArtAssetUtil.Save(so);
            return config;
        }

        private static void Fill(SerializedProperty array, Vector2Int[] cells)
        {
            array.arraySize = cells.Length;
            for (var i = 0; i < cells.Length; i++)
            {
                var sprite = ArtSprites.Named(Sheet, SheetName + "_r" + cells[i].x + "_c" + cells[i].y);
                ArtSprites.TryMeasure(sprite, Sheet, out var size, out var center);
                var entry = array.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_sprite").objectReferenceValue = sprite;
                entry.FindPropertyRelative("_contentSize").vector2Value = size;
                entry.FindPropertyRelative("_contentCenter").vector2Value = center;
                entry.FindPropertyRelative("_rotationOffsetDegrees").floatValue = RotationOffset;
            }
        }
    }
}
