using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Read-only facts about the run's lots and powers for the story cards. Bootstrap builds it from the definitions (the UI cannot see them).</summary>
    public interface IStoryFacts
    {
        /// <summary>The facts of a lot of the run; false for an unlisted lot.</summary>
        bool TryGetLot(LotId id, out StoryLotFacts facts);

        /// <summary>The facts of a story power; false for None or a power without a definition.</summary>
        bool TryGetPower(StoryPower power, out StoryPowerFacts facts);
    }
}
