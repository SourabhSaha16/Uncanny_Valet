using UncannyValet.Core.Watchers;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The watcher of the lot being played, if it has one. Registered by Bootstrap; the results screen (task 85) reads it for the god's verdict.</summary>
    public interface IWatcherSource
    {
        /// <summary>The watching character of this lot; false when the lot has none.</summary>
        bool TryGet(out WatcherDefinition watcher);
    }
}
