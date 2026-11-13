using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// Read-only view of the story powers' state and live numbers (the Powers module owns it). The HUD reads icon
    /// state from it; the Valet, Lot and other modules react to PowerActivated and read the level's numbers here,
    /// never reaching into the Powers module.
    /// </summary>
    public interface IPowerView
    {
        /// <summary>The state of an owned power; false when the power is not owned.</summary>
        bool TryGetState(StoryPower power, out PowerState state);

        /// <summary>The live numbers (F2 edits included) of a power at a level (1 or 2); false for None or an unknown level.</summary>
        bool TryGetValues(StoryPower power, int level, out PowerLevelValues values);
    }
}
