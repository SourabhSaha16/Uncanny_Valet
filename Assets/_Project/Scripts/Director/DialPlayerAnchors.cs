using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>Anchors of the two dials about how the director treats the player: Mercy and Skill Reward.</summary>
    public static class DialPlayerAnchors
    {
        private const TuningKey Mercy = TuningKey.DialMercy;
        private const TuningKey Skill = TuningKey.DialSkillReward;

        /// <summary>Every link of both dials.</summary>
        public static List<DialLink> All()
        {
            var all = new List<DialLink>();
            all.AddRange(MercyLinks());
            all.AddRange(SkillLinks());
            return all;
        }

        // More mercy: relief triggers sooner (lower thresholds, fewer angry patrons, longer memory), holds longer,
        // sends more plain cars, comes back sooner, and assist mode switches on from dial 7.5 (value reaches 0.5).
        private static DialLink[] MercyLinks() => new[]
        {
            new DialLink(Mercy, TuningKey.DirReliefWorkload, 2f, 0.6f),
            new DialLink(Mercy, TuningKey.DirReliefPressure, 1.15f, 0.7f),
            new DialLink(Mercy, TuningKey.DirReliefAngryCount, 2f, 0.5f),
            new DialLink(Mercy, TuningKey.DirReliefAngryWindow, 0.5f, 2f),
            new DialLink(Mercy, TuningKey.DirReliefHold, 0.4f, 2f),
            new DialLink(Mercy, TuningKey.DirReliefPlainCars, 0f, 2f),
            new DialLink(Mercy, TuningKey.DirReliefCooldown, 3f, 0.4f),
            new DialLink(Mercy, TuningKey.DirAssistDilemmaScale, 1.6f, 0.4f),
            new DialLink(Mercy, TuningKey.DirAssist, 0f, 1f, DialEffect.Add),
        };

        // More skill reward: form moves the target further, reacts over a shorter window, sooner and in bigger steps,
        // and every good-play signal weighs more.
        private static DialLink[] SkillLinks() => new[]
        {
            new DialLink(Skill, TuningKey.DirFormInfluence, 0f, 2.2f),
            new DialLink(Skill, TuningKey.DirFormTipWeight, 0.3f, 1.6f),
            new DialLink(Skill, TuningKey.DirFormAngryWeight, 0.5f, 1.5f),
            new DialLink(Skill, TuningKey.DirFormComboWeight, 0.3f, 2f),
            new DialLink(Skill, TuningKey.DirFormWindow, 1.5f, 0.6f),
            new DialLink(Skill, TuningKey.DirFormWarmup, 2f, 0.3f),
            new DialLink(Skill, TuningKey.DirFormMaxStep, 0.4f, 2f),
        };
    }
}
