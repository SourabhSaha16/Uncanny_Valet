using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.Lot;
using UncannyValet.Run;
using UnityEditor;

namespace UncannyValet.Tests.Run
{
    /// <summary>The shipped Vanaheim lot assets and the helpers the Vanaheim test fixtures share.</summary>
    internal static class VanaheimAssets
    {
        public const string TrainingPath = "Assets/_Project/Data/Run/VanaheimTrainingLot.asset";
        public const string CrazyPath = "Assets/_Project/Data/Run/VanaheimCrazyLot.asset";
        public const string RunPath = "Assets/_Project/Data/Run/RunDefinition.asset";
        public const string LotConfigPath = "Assets/_Project/Data/Lot/LotConfig.asset";

        public static LotDefinition LotAt(string path) => AssetDatabase.LoadAssetAtPath<LotDefinition>(path);

        public static LotConfig Config() => AssetDatabase.LoadAssetAtPath<LotConfig>(LotConfigPath);

        public static LotGeometry Geometry(LotDefinition lot) => new LotGeometry(Config(), lot.QueueSlots, lot.LayoutOptions);

        /// <summary>A tuning store seeded the way a lot scene does, over today's traffic numbers.</summary>
        public static TuningStore Seeded(LotDefinition lot)
        {
            var tuning = new TuningStore(new EventBus());
            tuning.Seed(TuningKey.TrafficMinOnScreen, 2f);
            tuning.Seed(TuningKey.TrafficTargetOnScreen, 4f);
            tuning.Seed(TuningKey.PlanLateEvChance, 0.5f);
            lot.SeedPacing(tuning, 1f);
            return tuning;
        }

        /// <summary>The arrival chance the lot lists for the modifier, or -1 when it does not list it.</summary>
        public static float Share(LotDefinition lot, ModifierId id)
        {
            foreach (var entry in lot.ModifierChances) if (entry.Modifier.Id == id) return entry.Chance;
            return -1f;
        }
    }
}
