using UncannyValet.Core.Events;

namespace UncannyValet.Core.Modifiers
{
    /// <summary>Pure mapping from a director car archetype to the service modifiers it stands for.</summary>
    public static class ArchetypeModifiers
    {
        public static ModifierSet Of(CarArchetype archetype)
        {
            switch (archetype)
            {
                case CarArchetype.Muddy: return ModifierSet.Of(ModifierId.Muddy);
                case CarArchetype.Ev: return ModifierSet.Of(ModifierId.NeedsCharge);
                case CarArchetype.MuddyEv: return ModifierSet.Of(ModifierId.Muddy, ModifierId.NeedsCharge);
                default: return ModifierSet.None;
            }
        }
    }
}
