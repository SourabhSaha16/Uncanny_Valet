namespace UncannyValet.Core.Music
{
    /// <summary>A layer of a lot's ambience. Stored as integers in assets: append-only.</summary>
    public enum AmbienceKind
    {
        /// <summary>A low murmur of a fair crowd.</summary>
        CrowdMurmur = 0,
        /// <summary>Faint fairground rides and a calliope far away.</summary>
        DistantRides = 1,
        /// <summary>Rain dripping from the eaves.</summary>
        DrippingEaves = 2,
        /// <summary>Thunder rumbling a long way off.</summary>
        DistantThunder = 3,
        /// <summary>A softly creaking waterwheel.</summary>
        WaterwheelCreak = 4,
        /// <summary>Not a loop: a bright splash when a car enters the wash station.</summary>
        WashSplash = 5,
    }
}
