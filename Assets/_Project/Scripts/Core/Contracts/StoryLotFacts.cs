using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>What the story cards need to know about a lot that its definition holds: its name, its watcher and its blessing threshold.</summary>
    public readonly struct StoryLotFacts
    {
        public readonly LotId Id;
        public readonly string LotName;
        /// <summary>The local god's or trainer's display name; empty when nobody watches the lot.</summary>
        public readonly string WatcherName;
        public readonly int Threshold;

        public StoryLotFacts(LotId id, string lotName, string watcherName, int threshold)
        {
            Id = id; LotName = lotName; WatcherName = watcherName ?? string.Empty; Threshold = threshold;
        }
    }
}
