using System.Collections.Generic;
using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.EditorTools.Watchers
{
    /// <summary>
    /// Grandma, the owner and trainer of Grandma's Diner (Midgard training): white bun, lavender dress, a flowered apron and a wooden spoon.
    /// Voice: warm and doting, with the occasional sharp word. First drafts for the owner's review; never overwritten once the assets exist.
    /// </summary>
    public static class GrandmaWatcher
    {
        public const string Speaker = "GRANDMA";

        public static WatcherLook Look()
        {
            var accessories = new List<WatcherAccessory>
            {
                new WatcherAccessory("Bun", new Vector2(15.5f, 3f), new Vector2(6f, 3f), new Color(0.9f, 0.9f, 0.95f)),
                new WatcherAccessory("Lens Left", new Vector2(12.5f, 10f), new Vector2(3f, 3f), new Color(0.2f, 0.2f, 0.3f)),
                new WatcherAccessory("Lens Right", new Vector2(18.5f, 10f), new Vector2(3f, 3f), new Color(0.2f, 0.2f, 0.3f)),
                new WatcherAccessory("Glasses Bridge", new Vector2(15.5f, 10f), new Vector2(3f, 1f), new Color(0.2f, 0.2f, 0.3f)),
                new WatcherAccessory("Apron", new Vector2(15.5f, 24f), new Vector2(9f, 8f), new Color(0.98f, 0.95f, 0.88f)),
                new WatcherAccessory("Apron Flower", new Vector2(15.5f, 23f), new Vector2(3f, 3f), new Color(0.95f, 0.4f, 0.5f), 1),
                new WatcherAccessory("Spoon Handle", new Vector2(24f, 22f), new Vector2(1f, 9f), new Color(0.6f, 0.4f, 0.2f)),
                new WatcherAccessory("Spoon Bowl", new Vector2(24f, 16.5f), new Vector2(3f, 3f), new Color(0.6f, 0.4f, 0.2f)),
            };
            return new WatcherLook(0, 1, 0, Color.white, Color.white, new Color(0.62f, 0.66f, 1f), accessories);
        }

        public static WatcherTextAsset Text()
        {
            var tips = new List<WatcherTip>
            {
                new WatcherTip(TipSituation.FirstArrival, "PARK HER NICE AND STRAIGHT, DEAR."),
                new WatcherTip(TipSituation.MuddyCar, "THAT CAR'S MUDDY. THE WASH, SWEETIE."),
                new WatcherTip(TipSituation.ChargeCar, "NEEDS A CHARGE, DEAR. PLUG IT IN."),
                new WatcherTip(TipSituation.PatienceLow, "THEY'RE GETTING GRUMPY. FETCH THAT CAR!"),
                new WatcherTip(TipSituation.QueueFull, "LOTS OF CARS WAITING. ONE AT A TIME."),
                new WatcherTip(TipSituation.CarDamaged, "GENTLY, DEAR. DENTS COST BLESSINGS."),
            };
            var comments = new Dictionary<WatcherEvent, string[]>
            {
                [WatcherEvent.Curse] = new[] { "OH DEAR. THAT WON'T DO AT ALL.", "SOMEBODY'S UPSET. FIX IT, SWEETIE.", "MIND THE MANNERS, DEAR." },
                [WatcherEvent.PerfectPark] = new[] { "LOVELY PARKING, DEAR!", "STRAIGHT AS A RULER! I'M PROUD.", "NOT A SCRATCH. BRAVO!" },
                [WatcherEvent.PerfectDropOff] = new[] { "RIGHT TO THE CURB. WONDERFUL!", "THAT PATRON IS BEAMING, DEAR.", "WHAT A SMOOTH HANDOVER!" },
                [WatcherEvent.BigCrash] = new[] { "OH MY! ARE YOU ALL RIGHT, DEAR?", "THAT'S HOW WE LEARN, SWEETIE.", "THEIR POOR CAR! OH, MY POOR WALL!" },
                [WatcherEvent.HalfwayToGoal] = new[] { "HALFWAY THERE! KEEP IT UP, DEAR.", "WELL DONE, SWEETIE! HALF DONE.", "HALF THE BLESSINGS ALREADY!" },
                [WatcherEvent.GoalReached] = new[] { "YOU DID IT, DEAR! I KNEW YOU WOULD.", "PIE FOR YOU TONIGHT!", "THAT'S ENOUGH BLESSINGS. WELL DONE!" },
                [WatcherEvent.Last30Seconds] = new[] { "THIRTY SECONDS LEFT, DEAR!", "NEARLY CLOSING TIME. ONE LAST PUSH!", "FINISH PRETTY, SWEETIE!" },
            };
            return WatcherTextAsset.Create("GrandmaText", tips, comments, null);
        }
    }
}
