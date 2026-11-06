using NUnit.Framework;
using UncannyValet.Audio;

namespace UncannyValet.Tests.Audio
{
    public class SynthMathTests
    {
        [Test]
        public void OscillatorsStayInRangeAndHitKnownPoints()
        {
            foreach (Waveform wave in new[] { Waveform.Sine, Waveform.Square, Waveform.Saw, Waveform.Triangle })
            {
                for (double p = -1.0; p < 2.0; p += 0.013)
                    Assert.LessOrEqual(System.Math.Abs(Oscillator.Sample(wave, p)), 1f + 1e-5f);
            }
            Assert.AreEqual(1f, Oscillator.Sample(Waveform.Sine, 0.25), 1e-5f);
            Assert.AreEqual(1f, Oscillator.Sample(Waveform.Square, 0.1), 1e-5f);
            Assert.AreEqual(-1f, Oscillator.Sample(Waveform.Square, 0.9), 1e-5f);
            Assert.AreEqual(-1f, Oscillator.Sample(Waveform.Saw, 0.0), 1e-5f);
            Assert.AreEqual(1f, Oscillator.Sample(Waveform.Triangle, 0.5), 1e-5f);
        }

        [Test]
        public void AdsrRisesDecaysSustainsAndReleasesToZero()
        {
            var env = new Adsr(0.1f, 0.1f, 0.5f, 0.2f);
            Assert.AreEqual(0f, env.Evaluate(0f, 1f), 1e-5f);
            Assert.AreEqual(1f, env.Evaluate(0.1f, 1f), 1e-5f);
            Assert.AreEqual(0.5f, env.Evaluate(0.5f, 1f), 1e-5f);
            Assert.AreEqual(0.25f, env.Evaluate(1.1f, 1f), 1e-5f);
            Assert.AreEqual(0f, env.Evaluate(1.2f, 1f), 1e-5f);
            Assert.AreEqual(1.2f, env.TotalTime(1f), 1e-5f);
        }

        [Test]
        public void NoiseIsDeterministicBoundedAndNotConstant()
        {
            var a = new NoiseSource(42u);
            var b = new NoiseSource(42u);
            float first = a.Next();
            Assert.AreEqual(first, b.Next());
            bool varied = false;
            for (int i = 0; i < 1000; i++)
            {
                float v = a.Next();
                Assert.That(v, Is.InRange(-1f, 1f));
                if (v != first) varied = true;
            }
            Assert.IsTrue(varied);
        }

        [Test]
        public void LowPassPassesDcAndBlocksHighFrequencies()
        {
            const int rate = 22050;
            float coefficient = OnePoleLowPass.Coefficient(500f, rate);
            var dc = new OnePoleLowPass();
            float settled = 0f;
            for (int i = 0; i < rate; i++) settled = dc.Process(1f, coefficient);
            Assert.AreEqual(1f, settled, 1e-3f);

            var hf = new OnePoleLowPass();
            float peak = 0f;
            for (int i = 0; i < rate; i++)
                peak = System.Math.Max(peak, System.Math.Abs(hf.Process(i % 2 == 0 ? 1f : -1f, coefficient)));
            Assert.Less(peak, 0.2f);
        }

        [Test]
        public void MidiToHzMatchesConcertPitch()
        {
            Assert.AreEqual(440f, Pitch.MidiToHz(69), 1e-3f);
            Assert.AreEqual(880f, Pitch.MidiToHz(81), 1e-2f);
        }

        [Test]
        public void MuteStateTogglesAndZeroesVolume()
        {
            var mute = new MuteState();
            Assert.AreEqual(0.5f, mute.Apply(0.5f));
            mute.Toggle();
            Assert.IsTrue(mute.Muted);
            Assert.AreEqual(0f, mute.Apply(0.5f));
            mute.Toggle();
            Assert.AreEqual(0.5f, mute.Apply(0.5f));
        }
    }
}
