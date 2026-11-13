using UncannyValet.Core.Ids;

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
            return CheckPacing(lot) ?? CheckModifierChances(lot) ?? CheckDials(lot) ?? CheckModifiers(lot) ?? CheckGrant(lot);
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
