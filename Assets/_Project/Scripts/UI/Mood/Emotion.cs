namespace UncannyValet.UI.Mood
{
    /// <summary>How a waiting patron feels about their pickup; the bubble, the plate flash and the CAR SLOTS row all show it.</summary>
    public enum Emotion
    {
        /// <summary>Plenty of patience left: green, slow flash.</summary>
        Happy,
        /// <summary>Patience running low: yellow, faster flash.</summary>
        Impatient,
        /// <summary>Patience nearly or fully gone: red, very fast flash.</summary>
        Angry
    }
}
