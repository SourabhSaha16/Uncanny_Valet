namespace UncannyValet.UI.Screens
{
    /// <summary>The words of the how-to-play screen, kept in one place. Highlight phrases are drawn in gold.</summary>
    public static class HowToPlayText
    {
        public const string Heading = "HOW TO PLAY";
        public const string ControlsHeading = "CONTROLS";
        public const string Prompt = "PRESS ANY KEY";

        public static readonly string[] Rules =
        {
            "You are a Valet at a busy diner",
            "Customers will DROP OFF their car and then PICK UP after a while",
            "You earn BLESSINGS only for PICK UPs, NOT for DROP OFFs",
            "Customer cars will sometimes have SPECIAL REQUESTs like car wash or EV charging",
            "Completing these requests will earn bonus BLESSINGS"
        };

        public static readonly string[] Controls =
        {
            "W (drive) A (left turn) S (reverse/brake) D (right turn)",
            "Shift (handbrake for drifting - sharper turns)",
            "E (to enter/exit car)"
        };

        public static readonly string[] Highlights = { "DROP OFF", "PICK UP", "BLESSINGS", "SPECIAL REQUEST" };
    }
}
