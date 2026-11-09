namespace UncannyValet.UI.Hud
{
    /// <summary>How worried the HUD should look about a timer.</summary>
    public enum AlarmLevel
    {
        /// <summary>Plenty of time.</summary>
        Calm,
        /// <summary>Running low: amber.</summary>
        Low,
        /// <summary>Expired or about to: orange-red.</summary>
        Danger
    }
}
