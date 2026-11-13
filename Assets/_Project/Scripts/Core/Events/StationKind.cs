namespace UncannyValet.Core.Events
{
    /// <summary>A lot station where a stopped car gets a service.</summary>
    public enum StationKind
    {
        /// <summary>Charges a car that needs a charge.</summary>
        Charging,

        /// <summary>Washes a Muddy car. Only one car fits.</summary>
        Wash
    }
}
