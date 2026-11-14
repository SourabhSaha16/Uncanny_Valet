namespace UncannyValet.Core.Watchers
{
    /// <summary>What a watching character is. Stored as an integer in assets: append-only.</summary>
    public enum WatcherKind
    {
        /// <summary>Not set; not a real watcher.</summary>
        None = 0,

        /// <summary>The local god of a crazy lot (Midgard: Odin in disguise as Mrs. Okafor).</summary>
        God = 1,

        /// <summary>The named local trainer of a training lot.</summary>
        Trainer = 2
    }
}
