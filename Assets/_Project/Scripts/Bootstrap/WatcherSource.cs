using UncannyValet.Core.Contracts;
using UncannyValet.Core.Watchers;

namespace UncannyValet.Bootstrap
{
    /// <summary>The plain <see cref="IWatcherSource"/>: the watcher of the lot being played, or none.</summary>
    public sealed class WatcherSource : IWatcherSource
    {
        private readonly WatcherDefinition _watcher;

        public WatcherSource(WatcherDefinition watcher) => _watcher = watcher;

        public bool TryGet(out WatcherDefinition watcher)
        {
            watcher = _watcher;
            return watcher != null;
        }
    }
}
