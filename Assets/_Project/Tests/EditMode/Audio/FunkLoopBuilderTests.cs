using NUnit.Framework;
using UncannyValet.Audio;

namespace UncannyValet.Tests.Audio
{
    public class FunkLoopBuilderTests
    {
        private static LoopSettings Settings() => new LoopSettings(22050, 104f, 45, 0.15f);

        [Test]
        public void LengthIsWholeBarsOfWholeBeatSamples()
        {
            var settings = Settings();
            float[] loop = FunkLoopBuilder.Build(settings);
            Assert.AreEqual(FunkPattern.Bars * LoopSettings.BeatsPerBar * settings.SamplesPerBeat, loop.Length);
            double expected = FunkPattern.Bars * LoopSettings.BeatsPerBar * 22050 * 60.0 / 104.0;
            Assert.AreEqual(expected, loop.Length, LoopSettings.BeatsPerBar * FunkPattern.Bars);
        }

        [Test]
        public void PeakIsWithinFullScaleAndLoopIsNotSilent()
        {
            float[] loop = FunkLoopBuilder.Build(Settings());
            float peak = 0f;
            foreach (float s in loop) peak = System.Math.Max(peak, System.Math.Abs(s));
            Assert.LessOrEqual(peak, 1f);
            Assert.Greater(peak, 0.5f);
        }

        [Test]
        public void LoopPointHasNoJump()
        {
            float[] loop = FunkLoopBuilder.Build(Settings());
            Assert.Less(System.Math.Abs(loop[0] - loop[loop.Length - 1]), 0.3f);
            Assert.Less(System.Math.Abs(loop[0]), 0.3f);
        }

        [Test]
        public void BuildIsDeterministic()
        {
            Assert.AreEqual(FunkLoopBuilder.Build(Settings()), FunkLoopBuilder.Build(Settings()));
        }

        [Test]
        public void PatternTablesMatchStepCount()
        {
            Assert.AreEqual(FunkPattern.Steps, FunkPattern.Bass.Length);
            Assert.AreEqual(FunkPattern.Steps, FunkPattern.Kick.Length);
            Assert.AreEqual(FunkPattern.Steps, FunkPattern.Snare.Length);
            Assert.AreEqual(FunkPattern.Steps, FunkPattern.StabSteps.Length);
            Assert.AreEqual(FunkPattern.Bars * LoopSettings.BeatsPerBar * LoopSettings.StepsPerBeat, FunkPattern.Steps);
        }
    }
}
