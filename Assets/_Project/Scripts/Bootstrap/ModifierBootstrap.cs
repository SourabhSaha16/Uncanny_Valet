using UncannyValet.Core;
using UncannyValet.Core.Modifiers;
using UncannyValet.Run;

namespace UncannyValet.Bootstrap
{
    /// <summary>Builds the lot's service modifier catalog and registers it for every module.</summary>
    public static class ModifierBootstrap
    {
        /// <summary>Registers <see cref="IModifierCatalog"/> from the lot's chance list (its order is the roll order) and returns it.</summary>
        public static IModifierCatalog Register(LotDefinition lot)
        {
            var catalog = lot.BuildModifierCatalog();
            Services.Register<IModifierCatalog>(catalog);
            return catalog;
        }
    }
}
