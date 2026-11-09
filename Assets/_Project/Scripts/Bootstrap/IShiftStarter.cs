namespace UncannyValet.Bootstrap
{
    /// <summary>Starts a shift. Implemented over the Night module's clock; faked in tests.</summary>
    public interface IShiftStarter
    {
        bool IsRunning { get; }
        void StartShift();
    }
}
