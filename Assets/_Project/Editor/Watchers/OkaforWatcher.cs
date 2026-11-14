using System.Collections.Generic;
using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.EditorTools.Watchers
{
    /// <summary>
    /// Mrs. Okafor, the diner boss of Midgard's crazy lot (Odin in disguise, revealed at the Asgard sunrise): a dark-skinned woman with white
    /// hair in a bright patterned dress, round glasses and a clipboard. Voice: stern, brisk, businesslike, with the odd hint that she has
    /// seen it all. These are the first drafts of the owner's review; the assets are never overwritten once they exist.
    /// </summary>
    public static class OkaforWatcher
    {
        public const string Speaker = "MRS. OKAFOR";

        public static WatcherLook Look()
        {
            var dress = new Color(1f, 0.42f, 0.28f);
            var dots = new Color(1f, 0.85f, 0.25f);
            var glass = new Color(0.1f, 0.08f, 0.14f);
            var accessories = new List<WatcherAccessory>
            {
                new WatcherAccessory("Dots Left", new Vector2(12f, 21f), new Vector2(2f, 2f), dots),
                new WatcherAccessory("Dots Right", new Vector2(19f, 20f), new Vector2(2f, 2f), dots),
                new WatcherAccessory("Dots Low", new Vector2(15.5f, 25f), new Vector2(2f, 2f), dots),
                new WatcherAccessory("Lens Left", new Vector2(12.5f, 10f), new Vector2(3f, 3f), glass),
                new WatcherAccessory("Lens Right", new Vector2(18.5f, 10f), new Vector2(3f, 3f), glass),
                new WatcherAccessory("Glasses Bridge", new Vector2(15.5f, 10f), new Vector2(3f, 1f), glass),
                new WatcherAccessory("Clipboard", new Vector2(25f, 21f), new Vector2(6f, 9f), new Color(0.55f, 0.36f, 0.18f)),
                new WatcherAccessory("Clipboard Paper", new Vector2(25f, 21.5f), new Vector2(4f, 7f), new Color(0.97f, 0.96f, 0.9f), 1),
                new WatcherAccessory("Clipboard Clip", new Vector2(25f, 17f), new Vector2(3f, 1f), new Color(0.75f, 0.75f, 0.8f), 2),
            };
            return new WatcherLook(3, 1, 0, Color.white, new Color(0.92f, 0.88f, 1f), dress, accessories);
        }

        public static WatcherTextAsset Text()
        {
            var tips = new List<WatcherTip>
            {
                new WatcherTip(TipSituation.FirstArrival, "KEYS IN, CAR OUT. I TIME EVERYTHING."),
                new WatcherTip(TipSituation.MuddyCar, "THAT ONE NEEDS A WASH, VALET."),
                new WatcherTip(TipSituation.ChargeCar, "RUNNING ON FUMES. CHARGE IT, VALET."),
                new WatcherTip(TipSituation.PatienceLow, "THAT PATRON IS ABOUT TO BLOW. MOVE!"),
                new WatcherTip(TipSituation.QueueFull, "QUEUE'S FULL. CLEAR IT, I'M WATCHING."),
                new WatcherTip(TipSituation.CarDamaged, "DENTS COST BLESSINGS. EASY ON THE WALLS."),
            };
            var comments = new Dictionary<WatcherEvent, string[]>
            {
                [WatcherEvent.Curse] = new[] { "THAT'S A CURSE ON THE HOUSE, VALET.", "I SAW THAT. SHAMEFUL.", "ANGRY PATRONS ARE BAD FOR BUSINESS." },
                [WatcherEvent.PerfectPark] = new[] { "NOT A LINE OUT OF PLACE. HMM.", "NEAT. I'VE SEEN WORSE. MUCH WORSE.", "NICE PARKING. I SUPPOSE." },
                [WatcherEvent.PerfectDropOff] = new[] { "SWIFT, SMOOTH, AT THE CURB. GOOD.", "THE PATRON IS PLEASED. SO AM I.", "THAT'S HOW A DROP-OFF IS DONE." },
                [WatcherEvent.BigCrash] = new[] { "MY INSURANCE WILL HEAR ABOUT THIS.", "A CAR OR A BATTERING RAM?", "OH, THE NOISE. THE PAPERWORK!" },
                [WatcherEvent.HalfwayToGoal] = new[] { "HALF THE BLESSINGS ARE IN. WELL DONE.", "HALFWAY. DON'T GET COMFY.", "HALFWAY. I'M ALMOST IMPRESSED." },
                [WatcherEvent.GoalReached] = new[] { "ENOUGH BLESSINGS. SPLENDID.", "GOAL MET. I HAD MY DOUBTS.", "THAT'LL DO, VALET. THAT'LL DO." },
                [WatcherEvent.Last30Seconds] = new[] { "THIRTY SECONDS. MAKE THEM COUNT.", "CLOSING TIME SOON. FINISH STRONG.", "TICK TOCK, VALET." },
            };
            var verdicts = new Dictionary<WatcherVerdict, string[]>
            {
                [WatcherVerdict.Failed] = new[] { "NOT ENOUGH. I EXPECTED MORE.", "THE FAIR IS DISAPPOINTED.", "TRY AGAIN. I'M NOT GOING ANYWHERE." },
                [WatcherVerdict.Cleared] = new[] { "ADEQUATE. YOU MAY CONTINUE.", "THE BLESSINGS WILL DO. THIS TIME.", "YOU'LL GO FAR, VALET." },
                [WatcherVerdict.ClearedWell] = new[] { "REMARKABLE. YOU HAVE MY ATTENTION.", "I'M SURPRISED. PLEASANTLY.", "FINE WORK. ALMOST GODLIKE." },
            };
            return WatcherTextAsset.Create("MrsOkaforText", tips, comments, verdicts);
        }
    }
}
