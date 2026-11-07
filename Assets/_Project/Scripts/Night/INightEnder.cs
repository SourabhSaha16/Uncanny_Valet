namespace UncannyValet.Night
{
    /// <summary>Lets a rule end the running night early through the clock's normal end path.</summary>
    public interface INightEnder
    {
        bool IsRunning { get; }

        /// <summary>Ends the night on the clock's next advance (publishing NightEnded once). No effect when not running.</summary>
        void EndAtNextTick();
    }
}
