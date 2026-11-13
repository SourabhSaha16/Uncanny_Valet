namespace UncannyValet.Core.Modifiers
{
    /// <summary>Pure lookups over a modifier set and a catalog.</summary>
    public static class ModifierLooks
    {
        /// <summary>True when any modifier of <paramref name="set"/> has the given look; false for a null catalog.</summary>
        public static bool Has(IModifierCatalog catalog, ModifierSet set, ModifierLook look)
        {
            if (catalog == null) return false;
            for (var i = 0; i < set.Count; i++)
                if (catalog.TryGet(set.At(i), out var def) && def.Look == look) return true;
            return false;
        }

        /// <summary>True when the definition of <paramref name="id"/> has the given look; false for a null catalog or an unknown id.</summary>
        public static bool Is(IModifierCatalog catalog, ModifierId id, ModifierLook look)
            => catalog != null && catalog.TryGet(id, out var def) && def.Look == look;
    }
}
