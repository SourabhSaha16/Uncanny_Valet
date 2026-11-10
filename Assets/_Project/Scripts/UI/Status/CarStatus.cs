namespace UncannyValet.UI.Status
{
    /// <summary>
    /// What the CAR SLOTS panel says about a parked car. To add one (for example Overheating): add the id here, one
    /// <see cref="ICarStatusRule"/> in <see cref="CarStatusResolver.CreateDefault"/> at its priority, and one entry in <see cref="CarStatusTable"/>.
    /// </summary>
    public enum CarStatus
    {
        Parked,
        Washing,
        Washed,
        Charging,
        Charged,
        PickUp,
        CustAngry
    }
}
