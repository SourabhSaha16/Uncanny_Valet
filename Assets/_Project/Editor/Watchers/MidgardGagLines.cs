using UncannyValet.Core.Watchers;

namespace UncannyValet.EditorTools.Watchers
{
    /// <summary>
    /// First drafts of the floating gag lines of the two Midgard lots, for the owner's review: in Grandma's Diner patrons mock Thor's name, at the
    /// fair the fairgoers think he is in costume. Never overwritten once the assets exist.
    /// </summary>
    public static class MidgardGagLines
    {
        public const string TrainingSpeaker = "PATRON";
        public const string CrazySpeaker = "FAIRGOER";

        public static readonly string[] Training =
        {
            "THOR? LIKE THE GOD? HA!",
            "NICE NAME, THUNDER BOY.",
            "THOR. SURE. AND I'M ODIN.",
            "DOES YOUR HAMMER PARK CARS?",
            "HEY THOR, WHERE'S YOUR CAPE?",
            "THOR THE VALET. THAT'S RICH.",
            "MIND THE LIGHTNING, THOR!",
            "WHO NAMES A KID THOR?",
        };

        public static readonly string[] Crazy =
        {
            "GREAT COSTUME! IS THAT A HAMMER?",
            "LOVE THE THOR OUTFIT!",
            "IS HE PART OF THE FAIR?",
            "BEST THOR COSTUME HERE!",
            "WHERE DID YOU GET THE CAPE?",
            "WOW, THE BEARD LOOKS REAL!",
            "PHOTO? YOU'RE THE BEST THOR!",
            "THOR! WIN ME A PLUSH TOY!",
        };

        public static GagLineAsset TrainingAsset() => GagLineAsset.Create("MidgardTrainingGagLines", TrainingSpeaker, Training);

        public static GagLineAsset CrazyAsset() => GagLineAsset.Create("MidgardCrazyGagLines", CrazySpeaker, Crazy);
    }
}
