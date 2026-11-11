using UnityEngine;

namespace UncannyValet.Cars.Fx
{
    /// <summary>
    /// Pure skid rules of docs/redesign/car_handling_spec.md section 6: is the car sliding (with hysteresis), how dark the
    /// marks are, and when hard braking begins. One instance per car; it remembers the previous step.
    /// </summary>
    public sealed class SkidRule
    {
        private bool _sliding;
        private bool _braking;

        /// <summary>True while the last step was a slide.</summary>
        public bool Sliding => _sliding;

        /// <summary>Judges one step. A slide starts above the threshold and ends only below <see cref="SkidSettings.ExitShare"/> of it.</summary>
        public SkidState Step(in SlipReading reading, in SkidSettings s)
        {
            var share = _sliding ? s.ExitShare : 1f;
            var now = Mathf.Abs(reading.Sideways) > s.SlideThreshold * share
                || (reading.Drift && reading.Speed > s.ShiftSlideSpeed * share);
            var started = now && !_sliding;
            var ended = _sliding && !now;
            _sliding = now;
            var intensity = now ? Mathf.Clamp01(reading.SlipAngleDegrees / s.FullDriftDegrees) : 0f;
            var opacity = now ? s.MaxOpacity * Mathf.Lerp(s.FaintShare, 1f, intensity) : 0f;
            return new SkidState(now, started, ended, HardBraking(reading, s), intensity, opacity);
        }

        /// <summary>Ends everything (driver left, car taken over); reports the slide end once if one was running.</summary>
        public SkidState Reset()
        {
            var ended = _sliding;
            _sliding = false;
            _braking = false;
            return new SkidState(false, false, ended, false, 0f, 0f);
        }

        // Braking = the throttle opposes the travel direction while fast; true only on the step it begins.
        private bool HardBraking(in SlipReading reading, in SkidSettings s)
        {
            var share = _braking ? s.ExitShare : 1f;
            var now = reading.Throttle * reading.Forward < 0f && Mathf.Abs(reading.Forward) > s.HardBrakeSpeed * share;
            var started = now && !_braking;
            _braking = now;
            return started;
        }
    }
}
