using System.Collections.Generic;

namespace UncannyValet.Core.Modifiers
{
    /// <summary>The plain <see cref="IModifierCatalog"/>: a fixed list of definitions looked up by id.</summary>
    public sealed class ModifierCatalog : IModifierCatalog
    {
        private readonly List<ServiceModifierDefinition> _all = new List<ServiceModifierDefinition>();
        private readonly ServiceModifierDefinition[] _byId = new ServiceModifierDefinition[ModifierSet.Capacity];

        /// <summary>Builds the catalog; a null entry or a repeated id is skipped.</summary>
        public ModifierCatalog(IEnumerable<ServiceModifierDefinition> definitions)
        {
            foreach (var d in definitions)
            {
                if (d == null || d.Id == ModifierId.None || _byId[(int)d.Id] != null) continue;
                _byId[(int)d.Id] = d;
                _all.Add(d);
            }
        }

        public IReadOnlyList<ServiceModifierDefinition> All => _all;

        public bool TryGet(ModifierId id, out ServiceModifierDefinition definition)
        {
            var n = (int)id;
            definition = n > 0 && n < _byId.Length ? _byId[n] : null;
            return definition != null;
        }
    }
}
