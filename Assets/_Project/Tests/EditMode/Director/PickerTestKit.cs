using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>Shared builders for the archetype picker fixtures.</summary>
    internal static class PickerTestKit
    {
        public const int Budget = 14;

        public static ArchetypePicker Picker(DirectorSettings settings, int seed)
            => new ArchetypePicker(new TestRandom(seed), DilemmaRules.CreateAll(settings.Rules), settings.Picker, ModifierTestKit.Catalog());

        /// <summary>A lot with the wash slot and/or the two EV bays unlocked (the wash in the given state).</summary>
        public static SnapshotBuilderForTests Stations(bool wash, bool bays, StationSlotState washState = StationSlotState.Free)
        {
            var b = new SnapshotBuilderForTests();
            if (wash) b.Slot(StationKind.Wash, washState);
            if (bays) b.Slot(StationKind.Charging, StationSlotState.Free);
            return b;
        }

        public static PickRequest Request(SnapshotBuilderForTests b, ScheduleReason band = ScheduleReason.OnTarget,
            bool relief = false, bool dilemmas = false, ArchetypeWeights? weights = null)
            => new PickRequest(b.Build(), weights ?? ArchetypeWeights.Diner, band, relief, dilemmas, Budget);
    }
}
