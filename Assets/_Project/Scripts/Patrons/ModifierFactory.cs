using System.Collections.Generic;
using UncannyValet.Core.Modifiers;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Builds the modifier instances of a new ticket from its set of ids, with the clear times current at that moment
    /// (the live tuning when given, else the definitions' own numbers).
    /// </summary>
    public sealed class ModifierFactory
    {
        private readonly IModifierCatalog _catalog;
        private readonly PatronTuning _live;

        /// <param name="live">The live numbers; null uses the definitions' values.</param>
        public ModifierFactory(IModifierCatalog catalog, PatronTuning live = null)
        {
            _catalog = catalog; _live = live;
        }

        public IModifierCatalog Catalog => _catalog;

        /// <summary>One instance per id of <paramref name="set"/> that the catalog knows, in id order.</summary>
        public List<ModifierInstance> Build(ModifierSet set)
        {
            var list = new List<ModifierInstance>(set.Count);
            for (var i = 0; i < set.Count; i++)
            {
                if (!_catalog.TryGet(set.At(i), out var def)) continue;
                var minutes = _live != null ? _live.ClearMinutes(def) : def.ClearMinutes;
                list.Add(new ModifierInstance(def.Id, def.ClearedAt, def.StartProgress01, minutes));
            }
            return list;
        }
    }
}
