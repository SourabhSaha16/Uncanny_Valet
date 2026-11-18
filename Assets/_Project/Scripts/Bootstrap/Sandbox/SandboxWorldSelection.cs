namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>Which of the sandbox's worlds is on screen. Pure rules for choosing by index or by a 1-based key number.</summary>
    public sealed class SandboxWorldSelection
    {
        public SandboxWorldSelection(int count, int current = 0)
        {
            Count = count < 0 ? 0 : count;
            Current = current >= 0 && current < Count ? current : 0;
        }

        /// <summary>Number of worlds.</summary>
        public int Count { get; }

        /// <summary>Index of the world on screen (0 when there are none).</summary>
        public int Current { get; private set; }

        /// <summary>Chooses a world by index. False (nothing changes) when the index is out of range or already current.</summary>
        public bool TrySelect(int index)
        {
            if (index < 0 || index >= Count || index == Current) return false;
            Current = index;
            return true;
        }

        /// <summary>Chooses a world by its key number (1 is the first world).</summary>
        public bool TrySelectNumber(int number) => TrySelect(number - 1);
    }
}
