using System.Collections.Generic;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// What the lot's modifiers ask the lot builder to put in the world. Each modifier adds its part (today the mud patch list);
    /// the scene builder hands the result to the lot builder. A new world object is a new list here, not a change to the old ones.
    /// </summary>
    public sealed class LotWorldSpec
    {
        private readonly List<MudPatch> _mudPatches = new List<MudPatch>();

        /// <summary>Every mud puddle, in the lot's own space.</summary>
        public IReadOnlyList<MudPatch> MudPatches => _mudPatches;

        /// <summary>Adds a mud puddle.</summary>
        public void AddMudPatch(MudPatch patch) => _mudPatches.Add(patch);
    }
}
