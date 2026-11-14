using UncannyValet.Core.Art;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Watchers;

namespace UncannyValet.Run
{
    /// <summary>The validation rules of a <see cref="LotDefinition"/>. Pure, so the tests and the setup tools share them.</summary>
    public static class LotDefinitionRules
    {
        /// <summary>True when the lot is usable; otherwise <paramref name="problem"/> names the first fault.</summary>
        public static bool TryValidate(LotDefinition lot, out string problem)
        {
            problem = Check(lot);
            return problem == null;
        }

        private static string Check(LotDefinition lot)
        {
            if (lot == null) return "the lot is missing";
            var name = lot.name;
            if (!lot.Id.IsValid) return name + ": its LotId is not a real lot (set world and kind; Asgard is the only Finale)";
            if (string.IsNullOrEmpty(lot.SceneName)) return name + ": no scene name";
            if (string.IsNullOrEmpty(lot.DisplayName)) return name + ": no display name";
            if (lot.Layout == LotLayoutKind.None) return name + ": no layout";
            if (lot.QueueSlots < 1 || lot.QueueSlots > LotDefinition.MaxQueueSlots)
                return name + ": the queue slot count must be 1 to " + LotDefinition.MaxQueueSlots;
            if (lot.BlessingThreshold < 1) return name + ": the blessing threshold must be at least 1";
            return CheckPacing(lot) ?? CheckPowerFill(lot) ?? CheckModifierChances(lot) ?? CheckDials(lot) ?? CheckModifiers(lot) ?? CheckGrant(lot) ?? CheckArt(lot) ?? CheckWatcher(lot) ?? CheckWorld(lot);
        }

        private static string CheckWorld(LotDefinition lot)
        {
            if (lot.AmbientPedestrians < 0 || lot.AmbientPedestrians > LotDefinition.MaxAmbientPedestrians)
                return lot.name + ": the crowd size must be 0 to " + LotDefinition.MaxAmbientPedestrians;
            if (lot.LayoutOptions.CornerClearance < 0f) return lot.name + ": the corner clearance cannot be negative";
            var options = lot.LayoutOptions;
            if (options.WashTroughs > 0 && !options.ServicesClosed) return lot.name + ": wash troughs need closed services (the grid's wash slot would be a second wash)";
            if (options.WashTroughs > 1 && options.ExtensionWidth <= 0f) return lot.name + ": more than one wash trough needs a widened lot";
            if (options.ExtensionWidth > 0f && options.TroughDistance <= 0f && options.WashTroughs > 0) return lot.name + ": troughs at the far end need a distance from the bays";
            return lot.GagLines != null ? lot.GagLines.Check() : null;
        }

        private static string CheckArt(LotDefinition lot)
        {
            if (lot.Palette != null && !lot.Palette.TryValidate(out var paletteProblem)) return lot.name + ": " + paletteProblem;
            if (lot.Props.Count > 0 && lot.Palette == null) return lot.name + ": it has props but no world palette";
            for (var i = 0; i < lot.Props.Count; i++)
                if (!PropRecipeRules.TryValidate(lot.Props[i].Recipe, out var propProblem)) return lot.name + ": prop " + i + ": " + propProblem;
            return lot.Lighting != null && !lot.Lighting.TryValidate(out var lightProblem) ? lot.name + ": " + lightProblem : null;
        }

        private static string CheckWatcher(LotDefinition lot)
        {
            var watcher = lot.Watcher;
            if (watcher == null) return null;
            if (watcher.World != lot.Id.World) return lot.name + ": the watcher " + watcher.name + " belongs to another world";
            var expected = lot.Id.Kind == LotKind.Training ? WatcherKind.Trainer : WatcherKind.God;
            return watcher.Kind == expected ? null : lot.name + ": the watcher must be a " + expected + " in this lot";
        }

        private static string CheckPacing(LotDefinition lot)
        {
            var p = lot.Pacing;
            if (p == null) return lot.name + ": no pacing";
            if (p.ShiftLengthSeconds <= 0f) return lot.name + ": the shift length must be above zero";
            if (p.ArrivalGapMinSeconds <= 0f) return lot.name + ": the shortest arrival gap must be above zero";
            if (p.ArrivalGapMaxSeconds < p.ArrivalGapMinSeconds) return lot.name + ": the longest arrival gap is below the shortest";
            if (p.MaxWaitingCars < 1) return lot.name + ": at least one car must be allowed to wait";
            if (p.ArrivalBudgetOverride < 0) return lot.name + ": the arrival budget cannot be negative (0 derives it)";
            return null;
        }

        private static string CheckPowerFill(LotDefinition lot)
            => lot.PowerFill != null && lot.PowerFill.IsSane ? null : lot.name + ": the power bar fill amounts must not be negative";

        private static string CheckModifierChances(LotDefinition lot)
        {
            var total = 0f;
            for (var i = 0; i < lot.ModifierChances.Count; i++)
            {
                var entry = lot.ModifierChances[i];
                if (!entry.IsValid) return lot.name + ": modifier chance entry " + i + " has no modifier or a chance outside 0..1";
                for (var j = 0; j < i; j++)
                    if (lot.ModifierChances[j].Modifier.Id == entry.Modifier.Id) return lot.name + ": modifier " + entry.Modifier.name + " is listed twice";
                total += entry.Chance;
            }
            return total > 1.0001f ? lot.name + ": the modifier chances add up to more than 1 (they are one draw in list order)" : null;
        }

        private static string CheckDials(LotDefinition lot)
        {
            for (var i = 0; i < lot.Dials.Count; i++)
            {
                if (!lot.Dials[i].IsValid) return lot.name + ": dial entry " + i + " is not a dial key or is off the 0..10 slider";
                for (var j = 0; j < i; j++)
                    if (lot.Dials[j].Dial == lot.Dials[i].Dial) return lot.name + ": dial " + lot.Dials[i].Dial + " is listed twice";
            }
            return null;
        }

        private static string CheckModifiers(LotDefinition lot)
        {
            for (var i = 0; i < lot.Modifiers.Count; i++)
            {
                if (lot.Modifiers[i] == null) return lot.name + ": modifier " + i + " is empty";
                for (var j = 0; j < i; j++)
                    if (lot.Modifiers[j] == lot.Modifiers[i]) return lot.name + ": modifier " + lot.Modifiers[i].name + " is listed twice";
            }
            return null;
        }

        private static string CheckGrant(LotDefinition lot)
        {
            if (!lot.Grant.IsValid) return lot.name + ": the power grant level must be 1 or 2";
            if (lot.Id.Kind == LotKind.Training && !lot.Grant.IsNone) return lot.name + ": a training lot grants no power";
            return null;
        }
    }
}
