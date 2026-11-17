using System.Collections.Generic;

namespace UncannyValet.Core.Modifiers
{
    /// <summary>The modifier definitions of the running lot, in roll order. Registered in Services by Bootstrap.</summary>
    public interface IModifierCatalog
    {
        /// <summary>Every definition, in the order arrivals are rolled.</summary>
        IReadOnlyList<ServiceModifierDefinition> All { get; }

        /// <summary>The definition of <paramref name="id"/>; false when the lot does not use it.</summary>
        bool TryGet(ModifierId id, out ServiceModifierDefinition definition);
    }
}
