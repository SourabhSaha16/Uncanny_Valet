using UncannyValet.Cars;
using UncannyValet.EditorTools.Art;
using UncannyValet.Lot;
using UnityEditor;

namespace UncannyValet.EditorTools
{
    /// <summary>Creates the CarHandlingConfig asset and copies the lot's aisle width into it (Cars cannot reference Lot).</summary>
    public static class HandlingAssetSetup
    {
        public const string ConfigPath = "Assets/_Project/Data/Cars/CarHandlingConfig.asset";

        /// <summary>Loads or creates the handling config and makes its MeasuredAisleWidth equal <paramref name="lot"/>'s aisle width.</summary>
        public static CarHandlingConfig LoadAndSync(LotConfig lot)
        {
            var config = ArtAssetUtil.LoadOrCreate<CarHandlingConfig>(ConfigPath);
            var so = new SerializedObject(config);
            so.FindProperty("_measuredAisleWidth").floatValue = lot.AisleWidth;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
            return config;
        }
    }
}
