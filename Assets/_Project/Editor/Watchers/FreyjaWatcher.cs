using System.Collections.Generic;
using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.EditorTools.Watchers
{
    /// <summary>
    /// Freyja, the Vanir goddess who watches Vanaheim's crazy lot from the barn door: golden hair, a green gown, a gold circlet and necklace
    /// and a sheaf of wheat. Voice: warm, theatrical and a little vain. First drafts for the owner's review; never overwritten once the assets exist.
    /// </summary>
    public static class FreyjaWatcher
    {
        public const string Speaker = "FREYJA";

        public static WatcherLook Look()
        {
            var gold = new Color(1f, 0.82f, 0.25f);
            var accessories = new List<WatcherAccessory>
            {
                new WatcherAccessory("Circlet", new Vector2(15.5f, 4.5f), new Vector2(9f, 1.5f), gold),
                new WatcherAccessory("Necklace", new Vector2(15.5f, 17.5f), new Vector2(6f, 1f), gold),
                new WatcherAccessory("Gem", new Vector2(15.5f, 19f), new Vector2(2f, 2f), new Color(0.9f, 0.25f, 0.4f), 1),
                new WatcherAccessory("Wheat Stalk", new Vector2(24.5f, 22f), new Vector2(1f, 10f), new Color(0.8f, 0.65f, 0.25f)),
                new WatcherAccessory("Wheat Ear", new Vector2(24.5f, 15f), new Vector2(3f, 5f), gold, 1),
            };
            return new WatcherLook(1, 3, 0, Color.white, new Color(1f, 0.92f, 0.55f), new Color(0.25f, 0.78f, 0.5f), accessories);
        }

        public static WatcherTextAsset Text()
        {
            var tips = new List<WatcherTip>
            {
                new WatcherTip(TipSituation.FirstArrival, "WELCOME TO THE FARM! PARK WITH CARE."),
                new WatcherTip(TipSituation.MuddyCar, "MUDDY WHEELS! SEND IT THROUGH THE WASH."),
                new WatcherTip(TipSituation.ChargeCar, "THIS ONE'S THIRSTY. GIVE IT A CHARGE."),
                new WatcherTip(TipSituation.PatienceLow, "PATIENCE IS RUNNING DRY. BE QUICK, LOVE."),
                new WatcherTip(TipSituation.QueueFull, "A FULL QUEUE! CLEAR IT, DARLING."),
                new WatcherTip(TipSituation.CarDamaged, "OUCH! EVERY DENT COSTS BLESSINGS."),
            };
            var comments = new Dictionary<WatcherEvent, string[]>
            {
                [WatcherEvent.Curse] = new[] { "CURSES ARE SO UNBECOMING, DARLING.", "SOMEONE'S SULKING. HOW DRAMATIC.", "I FELT THAT ONE FROM HERE." },
                [WatcherEvent.PerfectPark] = new[] { "GORGEOUS PARKING, DARLING!", "PERFECTLY PLACED. I ADORE IT.", "NOW THAT IS ELEGANCE." },
                [WatcherEvent.PerfectDropOff] = new[] { "A DROP-OFF FIT FOR A GODDESS!", "THEY'RE SMITTEN. WELL DONE, LOVE.", "SMOOTH AS HONEY. DELIGHTFUL." },
                [WatcherEvent.BigCrash] = new[] { "THE BARN FELT THAT, DARLING.", "WHAT A CRUNCH. DREADFUL!", "MY FARM! HAVE SOME DECENCY!" },
                [WatcherEvent.HalfwayToGoal] = new[] { "HALFWAY BLESSED. HOW DELIGHTFUL!", "THE HARVEST IS HALF IN, LOVE.", "KEEP GOING. THE FIELDS ARE WATCHING." },
                [WatcherEvent.GoalReached] = new[] { "THE FIELDS ARE BLESSED. WELL DONE!", "ENOUGH BLESSINGS. I'M DELIGHTED.", "A BOUNTIFUL HARVEST, VALET!" },
                [WatcherEvent.Last30Seconds] = new[] { "THIRTY SECONDS, DARLING. SHINE!", "THE SUN IS SETTING. FINISH WELL.", "ONE LAST FLOURISH, LOVE!" },
            };
            var verdicts = new Dictionary<WatcherVerdict, string[]>
            {
                [WatcherVerdict.Failed] = new[] { "A POOR HARVEST. DO TRY AGAIN, LOVE.", "THE FIELDS WANTED MORE.", "NOT YOUR BEST, DARLING." },
                [WatcherVerdict.Cleared] = new[] { "A FINE HARVEST. YOU MAY PASS.", "I'M QUITE FOND OF YOU NOW.", "YOU'VE EARNED MY FAVOUR." },
                [WatcherVerdict.ClearedWell] = new[] { "SPLENDID! YOU ARE A TREASURE.", "THE WHOLE FARM ADORES YOU.", "MY HEART IS YOURS. ALMOST." },
            };
            return WatcherTextAsset.Create("FreyjaText", tips, comments, verdicts);
        }
    }
}
