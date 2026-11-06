namespace UncannyValet.Core.Events
{
    /// <summary>A lot station where a stopped car gets a service.</summary>
    public enum StationKind
    {
        /// <summary>Charges an EV.</summary>
        Charging,

        /// <summary>Washes a dirty car. Only one car fits.</summary>
        Wash
    }
}
