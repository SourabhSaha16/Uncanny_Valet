using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>One entry of the title-screen lot picker.</summary>
    public readonly struct LotChoice
    {
        public readonly LotId Id;
        public readonly string Name;

        public LotChoice(LotId id, string name) { Id = id; Name = name; }
    }
}
