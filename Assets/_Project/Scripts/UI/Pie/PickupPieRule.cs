using System;
using UncannyValet.Core.Events;
using UncannyValet.UI.Mood;

namespace UncannyValet.UI.Pie
{
    /// <summary>
    /// Pure rules of the pickup-timer pie: it shows while a pickup timer runs, a wedge is cut away clockwise from 12 o'clock as the
    /// timer wears off (full at 0, empty at 1), and its colour is the patron's emotion at that point of the timer.
    /// </summary>
    public static class PickupPieRule
    {
        /// <summary>Degrees of the wedge already cut away, clockwise from 12 o'clock (0 = full pie, 360 = nothing left).</summary>
        public static float CutDegrees(float elapsed01) => Math.Max(0f, Math.Min(1f, elapsed01)) * 360f;

        /// <summary>The pre-built wedge step for this point of the timer: 0 = full pie, <paramref name="steps"/> = empty.</summary>
        public static int Step(float elapsed01, int steps)
            => Math.Max(0, Math.Min(steps, (int)Math.Round(CutDegrees(elapsed01) / 360f * steps)));

        /// <summary>True while the row's pickup timer runs and is not yet worn off, so the pie shows.</summary>
        public static bool IsShown(TicketRow row, int steps)
            => row.State == TicketRowState.PickupRequested && row.HasTimer && row.Timer == TimerKind.Pickup
               && !row.Expired && Step(row.Elapsed01, steps) < steps;

        /// <summary>The colour state of the pie: green happy, yellow impatient, red angry, from the HUD emotion thresholds.</summary>
        public static Emotion ColorState(TicketRow row, EmotionRule rule) => rule.Resolve(row);
    }
}
