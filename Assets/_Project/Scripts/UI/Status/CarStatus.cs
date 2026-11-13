namespace UncannyValet.UI.Status
{
    /// <summary>
    /// What the CAR SLOTS panel says about a parked car. The service statuses are generic: their words come from the
    /// modifier definition (Washing / Washed, Charging / Charged, later Heating / Heated). To add another kind of status:
    /// add the id here, one <see cref="ICarStatusRule"/> in <see cref="CarStatusResolver.CreateDefault"/> at its priority,
    /// and one entry in <see cref="CarStatusTable"/>.
    /// </summary>
    public enum CarStatus
    {
        Parked,
        Servicing,
        Serviced,
        PickUp,
        CustAngry,
        WillNotBless
    }
}
