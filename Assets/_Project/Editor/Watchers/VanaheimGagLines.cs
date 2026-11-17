using UncannyValet.Core.Watchers;

namespace UncannyValet.EditorTools.Watchers
{
    /// <summary>
    /// First drafts of the floating gag lines of the two Vanaheim lots, for the owner's review: in Freyr's barn the villagers chat about the
    /// harvest and the strange valet, at the storm feast the guests drink, eat and sniff the air. Never overwritten once the assets exist.
    /// </summary>
    public static class VanaheimGagLines
    {
        public const string TrainingSpeaker = "VILLAGER";
        public const string CrazySpeaker = "VANIR";

        public static readonly string[] Training =
        {
            "FRESH HAY! WATCH THE BALES, LAD.",
            "THE WHEAT IS RIPE. GOOD YEAR.",
            "NICE CAR. DOES IT RUN ON OATS?",
            "MIND THE BASKETS, BIG FELLA!",
            "A VALET AT THE BARN? HOW ODD.",
            "FREYR BLESS THE HARVEST!",
            "MY SACKS ARE HEAVY. SO IS YOUR FOOT!",
            "WATCH THE FENCE POSTS, THUNDER BOY.",
        };

        public static readonly string[] Crazy =
        {
            "SMELLS LIKE THUNDER IN HERE",
            "MORE MEAD! THE STORM HAS PASSED!",
            "WATCH THE PUDDLES, BIG FELLA!",
            "THE BARN SURVIVED. BARELY.",
            "TRY THE ROAST BOAR!",
            "THAT WAS SOME STORM LAST NIGHT.",
            "MUD IS FOR PIGS, NOT CARS!",
            "HARVEST FEAST! EAT UNTIL YOU POP!",
        };

        public static GagLineAsset TrainingAsset() => GagLineAsset.Create("VanaheimTrainingGagLines", TrainingSpeaker, Training);

        public static GagLineAsset CrazyAsset() => GagLineAsset.Create("VanaheimCrazyGagLines", CrazySpeaker, Crazy);
    }
}
