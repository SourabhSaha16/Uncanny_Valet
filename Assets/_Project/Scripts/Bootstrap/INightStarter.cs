namespace UncannyValet.Bootstrap
{
    /// <summary>Starts a night. Implemented over the Night module's clock; faked in tests.</summary>
    public interface INightStarter
    {
        bool IsRunning { get; }
        void StartNight(int night);
    }
}
