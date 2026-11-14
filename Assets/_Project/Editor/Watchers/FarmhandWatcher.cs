using System.Collections.Generic;
using UncannyValet.Core.Watchers;
using UnityEngine;

namespace UncannyValet.EditorTools.Watchers
{
    /// <summary>
    /// Arni the Vanir farmhand, the trainer of Vanaheim's training lot: overalls, a straw hat and a pitchfork. Voice: gruff, earthy and short.
    /// The name is a draft for the owner to change. First drafts; never overwritten once the assets exist.
    /// </summary>
    public static class FarmhandWatcher
    {
        public const string Speaker = "FARMHAND ARNI";

        public static WatcherLook Look()
        {
            var straw = new Color(0.9f, 0.76f, 0.35f);
            var wood = new Color(0.55f, 0.36f, 0.18f);
            var iron = new Color(0.62f, 0.64f, 0.68f);
            var accessories = new List<WatcherAccessory>
            {
                new WatcherAccessory("Hat Brim", new Vector2(15.5f, 6f), new Vector2(15f, 2f), straw),
                new WatcherAccessory("Hat Crown", new Vector2(15.5f, 4f), new Vector2(8f, 3f), straw, 1),
                new WatcherAccessory("Pitchfork Handle", new Vector2(25f, 21f), new Vector2(1f, 14f), wood),
                new WatcherAccessory("Pitchfork Head", new Vector2(25f, 13.5f), new Vector2(5f, 1f), iron, 1),
                new WatcherAccessory("Pitchfork Tine Left", new Vector2(23f, 11.5f), new Vector2(1f, 3f), iron, 1),
                new WatcherAccessory("Pitchfork Tine Right", new Vector2(27f, 11.5f), new Vector2(1f, 3f), iron, 1),
            };
            return new WatcherLook(4, 0, 4, Color.white, Color.white, Color.white, accessories);
        }

        public static WatcherTextAsset Text()
        {
            var tips = new List<WatcherTip>
            {
                new WatcherTip(TipSituation.FirstArrival, "KEYS IN. PARK HER STRAIGHT IN THE BAY."),
                new WatcherTip(TipSituation.MuddyCar, "MUDDY ONE. SEND HER THROUGH THE WASH."),
                new WatcherTip(TipSituation.ChargeCar, "BATTERY'S DRY. HOOK HER TO THE CHARGER."),
                new WatcherTip(TipSituation.PatienceLow, "THEY'RE LOSING PATIENCE. HURRY IT UP."),
                new WatcherTip(TipSituation.QueueFull, "QUEUE'S FULL. TAKE 'EM ONE BY ONE."),
                new WatcherTip(TipSituation.CarDamaged, "EASY NOW. DENTS COST YOU BLESSINGS."),
            };
            var comments = new Dictionary<WatcherEvent, string[]>
            {
                [WatcherEvent.Curse] = new[] { "WELL, THAT WENT SOUR.", "THAT'LL LEAVE A MARK.", "ANGRY FOLK. HAPPENS." },
                [WatcherEvent.PerfectPark] = new[] { "NICE AND STRAIGHT. GOOD WORK.", "CAN'T FAULT THAT PARKING.", "RIGHT IN THE MIDDLE. SHARP." },
                [WatcherEvent.PerfectDropOff] = new[] { "RIGHT AT THE CURB. GOOD.", "THAT PATRON'S HAPPY. GOOD WORK.", "SMOOTH HANDOVER." },
                [WatcherEvent.BigCrash] = new[] { "WHOA! MIND THE FENCE!", "THAT'LL BUFF OUT. MAYBE.", "WATCH THE WALLS!" },
                [WatcherEvent.HalfwayToGoal] = new[] { "HALFWAY. STEADY NOW.", "HALF THE BLESSINGS. KEEP AT IT.", "GOOD PACE. KEEP GOING." },
                [WatcherEvent.GoalReached] = new[] { "THAT'S ENOUGH BLESSINGS. WELL DONE.", "DONE AND DUSTED.", "GOOD HARVEST TONIGHT." },
                [WatcherEvent.Last30Seconds] = new[] { "THIRTY SECONDS. PUSH ON.", "NEARLY CLOSING. LAST PUSH!", "ALMOST DONE. STAY SHARP." },
            };
            return WatcherTextAsset.Create("FarmhandText", tips, comments, null);
        }
    }
}
