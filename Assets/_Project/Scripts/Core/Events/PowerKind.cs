namespace UncannyValet.Core.Events
{
    /// <summary>The three valet powers. The numeric value is the meter level that unlocks the power.</summary>
    public enum PowerKind
    {
        /// <summary>Level 1: faster valet with the barge-through rule for a few seconds.</summary>
        SpeedBoost = 1,

        /// <summary>Level 2: the world stops for a short time; the valet and the driven car keep moving.</summary>
        FreezeTime = 2,

        /// <summary>Level 3: completes every pending job at once and pays the patrons' tips.</summary>
        ParkSmash = 3,
    }
}
