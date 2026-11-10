using System.Collections.Generic;
using UncannyValet.Cars;
using UncannyValet.Traffic;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.Art
{
    /// <summary>
    /// Creates Data/Traffic/TrafficConfig.asset on first run: the nose-up civilian car sprites of the CarArtConfig, a few
    /// tints, a scale that fits them to the lot car footprint and a car length equal to the lot cars. Later runs refresh
    /// the sprites, scale and length only, so tuned numbers survive.
    /// </summary>
    internal static class TrafficArtBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/Traffic/TrafficConfig.asset";
        private const int SortingOrder = 8; // under the lot cars (CarSorting.Body) and above the road scenery

        private static readonly Color[] Tints =
        {
            Color.white,
            new Color(0.75f, 0.85f, 1f),
            new Color(1f, 0.85f, 0.75f),
            new Color(0.8f, 1f, 0.85f),
        };

        public static TrafficConfig Build(CarArtConfig carArt, CarDefinition car)
        {
            var created = AssetDatabase.LoadAssetAtPath<TrafficConfig>(AssetPath) == null;
            var config = ArtAssetUtil.LoadOrCreate<TrafficConfig>(AssetPath);
            var so = new SerializedObject(config);
            var sprites = new List<Sprite>();
            var length = CollectSprites(carArt, sprites);
            ArtAssetUtil.SetSprites(so, "_sprites", sprites);
            so.FindProperty("_carLength").floatValue = car.Size.y;
            so.FindProperty("_carScale").floatValue = CarArtMath.UniformScale(new Vector2(0f, length), car.Size);
            if (created) FillDefaults(so);
            ArtAssetUtil.Save(so);
            return config;
        }

        // Adds every regular sprite and returns the mean opaque length of the sprites (world units at scale 1).
        private static float CollectSprites(CarArtConfig carArt, List<Sprite> sprites)
        {
            var so = new SerializedObject(carArt);
            var cars = so.FindProperty("_cars");
            var total = 0f;
            for (var i = 0; i < cars.arraySize; i++)
            {
                var entry = cars.GetArrayElementAtIndex(i);
                sprites.Add((Sprite)entry.FindPropertyRelative("_sprite").objectReferenceValue);
                total += entry.FindPropertyRelative("_contentSize").vector2Value.y;
            }
            return cars.arraySize == 0 ? 0f : total / cars.arraySize;
        }

        private static void FillDefaults(SerializedObject so)
        {
            so.FindProperty("_sortingOrder").intValue = SortingOrder;
            var tints = so.FindProperty("_tints");
            tints.arraySize = Tints.Length;
            for (var i = 0; i < Tints.Length; i++) tints.GetArrayElementAtIndex(i).colorValue = Tints[i];
        }

    }
}
