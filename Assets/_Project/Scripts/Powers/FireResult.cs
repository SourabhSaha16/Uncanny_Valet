namespace UncannyValet.Powers
{
    /// <summary>Why a power did or did not start when its key was pressed.</summary>
    public enum FireResult
    {
        /// <summary>The power started and a bucket was spent.</summary>
        Started,
        /// <summary>The power is not owned (level 0).</summary>
        NotOwned,
        /// <summary>No behaviour is registered for the power yet.</summary>
        NoPower,
        /// <summary>The power is running or cooling down.</summary>
        NotReady,
        /// <summary>The power bar has no open bucket.</summary>
        NoBucket,
        /// <summary>The power said it cannot start now (for example no target); nothing was spent.</summary>
        CannotActivate,
    }
}
