using UncannyValet.Audio;
using UncannyValet.Cars;
using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Lot;
using UncannyValet.Patrons;
using UncannyValet.Powers;
using UncannyValet.Run;
using UncannyValet.Traffic;
using UncannyValet.UI.Mood;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Copies every tuning number from the config assets into the store, once, at startup.</summary>
    public static class TuningSeeder
    {
        /// <param name="spotCount">Spots in the lot; used when the plan says 0 ("all spots open").</param>
        /// <param name="director">The Patron Director's numbers; null skips them (its keys then stay unseeded).</param>
        /// <param name="profile">The lot's director profile, which supplies the arrival budget.</param>
        /// <param name="traffic">The road traffic config; null seeds the config defaults.</param>
        /// <param name="pedestrians">How many ambient pedestrians start active (the F2 panel can change it).</param>
        /// <param name="bus">When given, the returned link re-derives the director numbers whenever a dial moves.</param>
        /// <param name="handling">The car handling config; null seeds the config defaults.</param>
        /// <param name="audio">The audio config (tyre sound keys); null seeds the config defaults.</param>
        /// <param name="powers">The valet power config; null seeds the config defaults.</param>
        /// <param name="perfect">The Perfect Park and drop-off thresholds (the lot config's); null seeds the defaults.</param>
        /// <param name="queueClear">How far from the queue a car must rest for its stay to start; null seeds the default.</param>
        /// <param name="emotions">The pickup emotion levels (angry, will not tip); null seeds the config defaults.</param>
        /// <param name="lot">The lot's overrides (shift length, arrival pacing, modifier chances, dial positions); null keeps the shared defaults.</param>
        /// <param name="modifiers">The modifier definitions; they seed the clear time and bonus keys. Null uses the shipped defaults.</param>
        /// <returns>The dial link (the caller disposes it), or null when no bus was given.</returns>
        public static DifficultyDialsLink Seed(ITuning t, ShiftConfig shift, PatronConfig patrons, TipConfig tips, int spotCount,
            DirectorConfig director = null, DirectorProfile profile = null, int pedestrians = 0, TrafficConfig traffic = null,
            IEventBus bus = null, CarHandlingConfig handling = null, AudioConfig audio = null, PowerBarConfig powers = null,
            PerfectSettings perfect = null, HudEmotionConfig emotions = null, QueueClearSettings queueClear = null,
            LotDefinition lot = null, IModifierCatalog modifiers = null)
        {
            SeedEmotions(t, emotions);
            CameraShake.SeedDefaults(t);
            new PerfectTuning(perfect ?? new PerfectSettings()).Seed(t);
            SeedTraffic(t, traffic);
            SeedHandling(t, handling);
            SeedTyreSound(t, audio);
            SeedPowers(t, powers);
            t.Seed(TuningKey.PedestrianCount, pedestrians);
            if (director != null && profile != null) director.SeedInto(t, profile);

            t.Seed(TuningKey.GameMinutesPerSecond, shift.GameMinutesPerSecond);
            t.Seed(TuningKey.ShiftLengthSeconds, shift.ShiftLengthSeconds);

            var service = patrons.Tuning;
            t.Seed(TuningKey.PickupWindowMinutes, patrons.PickupWindowMinutes);
            t.Seed(TuningKey.StayMinMinutes, patrons.StayRange.x);
            t.Seed(TuningKey.StayMaxMinutes, patrons.StayRange.y);
            t.Seed(TuningKey.ArrivalGapMinMinutes, patrons.ArrivalIntervalRange.x);
            t.Seed(TuningKey.ArrivalGapMaxMinutes, patrons.ArrivalIntervalRange.y);
            t.Seed(TuningKey.ImpatientChance, patrons.ImpatientChance);
            t.Seed(TuningKey.ImpatientWindowScale, patrons.ImpatientWindowScale);
            t.Seed(TuningKey.QueueMaxWaitMinutes, service.QueueWaitMinutes);
            t.Seed(TuningKey.QueueClearDistance, (queueClear ?? new QueueClearSettings()).ClearDistance);

            t.Seed(TuningKey.BaseTip, patrons.BaseTip);
            SeedModifiers(t, modifiers ?? ModifierDefaults.CreateCatalog());
            t.Seed(TuningKey.SpeedWindowMinutes, tips.SpeedBonusWindowMinutes);

            var plan = shift.Plan;
            t.Seed(TuningKey.PlanMaxWaitingCars, plan.MaxWaitingCars);
            t.Seed(TuningKey.PlanArrivalGapScale, plan.ArrivalGapScale);
            t.Seed(TuningKey.PlanActiveSpots, plan.ActiveSpots > 0 ? plan.ActiveSpots : spotCount);
            if (lot == null) SeedDefaultChances(t);
            t.Seed(TuningKey.PlanLateEvChance, plan.LateEvChance);
            t.Seed(TuningKey.PlanLateEvStart, plan.LateEvStart);
            lot?.SeedPacing(t, shift.GameMinutesPerSecond); // the lot's numbers and arrival budget win over the shared defaults
            return SeedDials(t, director, bus, lot);
        }

        // Each modifier's clear time and bonus live in its definition; the keys are what the F2 panel edits.
        private static void SeedModifiers(ITuning t, IModifierCatalog modifiers)
        {
            for (var i = 0; i < modifiers.All.Count; i++)
            {
                var def = modifiers.All[i];
                t.Seed(def.ClearMinutesKey, def.ClearMinutes);
                t.Seed(def.BonusKey, def.Bonus);
            }
        }

        // Without a lot the diner's chances apply (the lot's own list replaces them).
        private static void SeedDefaultChances(ITuning t)
        {
            t.Seed(TuningKey.ModifierChanceNeedsCharge, ModifierDefaults.NeedsChargeChance);
            t.Seed(TuningKey.ModifierChanceMuddy, ModifierDefaults.MuddyChance);
        }

        // Dials start at their default position and the mapper runs once, so every derived number is consistent at startup.
        private static DifficultyDialsLink SeedDials(ITuning t, DirectorConfig director, IEventBus bus, LotDefinition lot)
        {
            var config = director != null ? director.Dials : null;
            var temporary = config == null;
            if (temporary)
            {
                if (director != null) Debug.LogWarning("DirectorConfig has no Difficulty Dials Config: run Uncanny Valet/Setup/Build Everything. Using the built-in anchors.", director);
                config = ScriptableObject.CreateInstance<DifficultyDialsConfig>();
            }
            var dials = new DifficultyDials(config);
            dials.SeedDials(t);
            lot?.SeedDialPositions(t);
            if (temporary) Object.DestroyImmediate(config);
            return bus != null ? new DifficultyDialsLink(bus, t, dials) : null;
        }

        private static void SeedHandling(ITuning t, CarHandlingConfig handling)
        {
            var config = handling != null ? handling : ScriptableObject.CreateInstance<CarHandlingConfig>();
            new CarHandlingLive(config).Seed(t);
            if (handling == null) Object.DestroyImmediate(config);
        }

        private static void SeedEmotions(ITuning t, HudEmotionConfig emotions)
        {
            var config = emotions != null ? emotions : ScriptableObject.CreateInstance<HudEmotionConfig>();
            t.Seed(TuningKey.EmotionAngryFrom, config.AngryFrom);
            t.Seed(TuningKey.EmotionWillNotTipFrom, config.WillNotTipFrom);
            if (emotions == null) Object.DestroyImmediate(config);
        }

        private static void SeedPowers(ITuning t, PowerBarConfig powers)
        {
            var config = powers != null ? powers : ScriptableObject.CreateInstance<PowerBarConfig>();
            config.SeedInto(t);
            if (powers == null) Object.DestroyImmediate(config);
        }

        private static void SeedTyreSound(ITuning t, AudioConfig audio)
        {
            var config = audio != null ? audio : ScriptableObject.CreateInstance<AudioConfig>();
            t.Seed(TuningKey.TyreSoundVolume, config.TyreVolume);
            t.Seed(TuningKey.TyreSoundSharpness, config.TyreSharpness);
            if (audio == null) Object.DestroyImmediate(config);
        }

        private static void SeedTraffic(ITuning t, TrafficConfig traffic)
        {
            var config = traffic != null ? traffic : ScriptableObject.CreateInstance<TrafficConfig>();
            t.Seed(TuningKey.TrafficSpeedScale, config.SpeedScale);
            t.Seed(TuningKey.TrafficEntrySpeed, config.EntrySpeed);
            t.Seed(TuningKey.TrafficExitSpeed, config.ExitSpeed);
            t.Seed(TuningKey.TrafficMinOnScreen, config.MinOnScreen);
            t.Seed(TuningKey.TrafficTargetOnScreen, config.TargetOnScreen);
            if (traffic == null) Object.DestroyImmediate(config);
        }
    }
}
