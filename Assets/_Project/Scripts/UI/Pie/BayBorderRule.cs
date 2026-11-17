using System;
using UncannyValet.Core.Events;
using UncannyValet.UI.Mood;

namespace UncannyValet.UI.Pie
{
    /// <summary>Pure rules of the bay border flash: when it runs, how fast it blinks and whether a blink is lit at a moment.</summary>
    public static class BayBorderRule
    {
        /// <summary>True while the row's pickup timer runs (the same moment the pie shows), so its bay flashes in the timer colour.</summary>
        public static bool TimerFlashes(TicketRow row)
            => row.Spot > 0 && row.State == TicketRowState.PickupRequested && row.HasTimer && row.Timer == TimerKind.Pickup && !row.Expired;

        /// <summary>Blinks per second for the patron's emotion; an angrier patron blinks faster.</summary>
        public static float Hz(Emotion emotion, BayBorderSettings settings)
        {
            switch (emotion)
            {
                case Emotion.Angry: return settings.AngryHz;
                case Emotion.Impatient: return settings.ImpatientHz;
                default: return settings.HappyHz;
            }
        }

        /// <summary>True during the lit half of each blink period; blinks start lit at <paramref name="startSeconds"/>.</summary>
        public static bool IsLit(float nowSeconds, float startSeconds, float hz)
        {
            var phase = (nowSeconds - startSeconds) * hz;
            return phase - (float)Math.Floor(phase) < 0.5f;
        }

        /// <summary>True while the green flash after a clean park is still running.</summary>
        public static bool ParkedFlashRuns(float nowSeconds, float startSeconds, float durationSeconds)
            => nowSeconds - startSeconds < durationSeconds;
    }
}
