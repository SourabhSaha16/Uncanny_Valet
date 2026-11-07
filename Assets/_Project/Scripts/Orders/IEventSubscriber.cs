namespace UncannyValet.Orders
{
    /// <summary>A plain class that listens to the event bus between Subscribe and Unsubscribe (OnEnable / OnDisable).</summary>
    public interface IEventSubscriber
    {
        void Subscribe();
        void Unsubscribe();
    }
}
