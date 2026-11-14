using System;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Music;

namespace UncannyValet.Tests.Audio
{
    /// <summary>Theme validation and the generated loop: length, tempo, every instrument audible, the crazy theme faster.</summary>
    public sealed class MusicThemeTests
    {
        private const int Rate = 22050;
        private ThemeFixtures _f;

        [SetUp] public void SetUp() => _f = new ThemeFixtures();
        [TearDown] public void TearDown() => _f.Dispose();

        private static float Peak(float[] samples)
        {
            var peak = 0f;
            foreach (var s in samples) peak = Math.Max(peak, Math.Abs(s));
            return peak;
        }

        [Test]
        public void AGoodThemeValidates()
        {
            var theme = _f.Theme(voices: ThemeVoice.FolkGuitar);
            Assert.IsTrue(theme.TryValidate(out var problem), problem);
        }

        [Test]
        public void ABadTempoIsRejected()
        {
            Assert.IsFalse(_f.Theme(bpm: 10f, voices: ThemeVoice.Shaker).TryValidate(out var problem));
            StringAssert.Contains("tempo", problem);
        }

        [Test]
        public void ANoteListOfTheWrongLengthIsRejected()
        {
            var theme = _f.Theme(voices: ThemeVoice.Shaker);
            var so = new UnityEditor.SerializedObject(theme);
            so.FindProperty("_melody").arraySize = 5;
            so.ApplyModifiedPropertiesWithoutUndo();
            Assert.IsFalse(theme.TryValidate(out var problem));
            StringAssert.Contains("melody", problem);
        }

        [Test]
        public void AThemeWithNoInstrumentsNeedsAClipOverride()
        {
            Assert.IsFalse(_f.Theme().TryValidate(out _));
            Assert.IsTrue(_f.Theme(clip: _f.Clip()).TryValidate(out var problem), problem);
        }

        [Test]
        public void AnInstrumentListedTwiceIsRejected()
        {
            Assert.IsFalse(_f.Theme(voices: new[] { ThemeVoice.Clap, ThemeVoice.Clap }).TryValidate(out _));
        }

        [Test]
        public void TheLoopIsAWholeNumberOfBeatsAtTheThemeTempo()
        {
            var theme = _f.Theme(bpm: 90f, voices: ThemeVoice.FolkGuitar);
            var loop = ThemeLoopBuilder.Build(theme, Rate);
            var beat = (int)Math.Round(Rate * 60f / 90f);
            Assert.AreEqual(8 * beat, loop.Length);
            Assert.AreEqual(loop.Length, ThemeLoopBuilder.LengthSamples(theme, Rate));
        }

        [Test]
        public void ACrazyThemeAtAHigherTempoIsShorterThanItsTrainingTheme()
        {
            var training = ThemeLoopBuilder.LengthSamples(_f.Theme(bpm: 84f, voices: ThemeVoice.FolkGuitar), Rate);
            var crazy = ThemeLoopBuilder.LengthSamples(_f.Theme(bpm: 118f, voices: ThemeVoice.FolkGuitar), Rate);
            Assert.Less(crazy, training);
        }

        [Test]
        public void EveryInstrumentAloneIsAudibleAndWithinFullScale()
        {
            foreach (ThemeVoice voice in Enum.GetValues(typeof(ThemeVoice)))
            {
                var loop = ThemeLoopBuilder.Build(_f.Theme(minor: voice == ThemeVoice.FiddleSaw, voices: voice), Rate);
                Assert.AreEqual(0.9f, Peak(loop), 1e-3f, voice.ToString());
            }
        }

        [Test]
        public void TheClipOverrideIsReadFromTheAsset()
        {
            var clip = _f.Clip();
            Assert.AreSame(clip, _f.Theme(clip: clip, voices: ThemeVoice.Shaker).ClipOverride);
            Assert.IsNull(_f.Theme(voices: ThemeVoice.Shaker).ClipOverride);
        }

        [Test]
        public void ADuplicateAmbienceLayerIsRejected()
        {
            Assert.IsNull(_f.Ambience(AmbienceKind.CrowdMurmur, AmbienceKind.DistantRides).Check());
            Assert.IsNotNull(_f.Ambience(AmbienceKind.CrowdMurmur, AmbienceKind.CrowdMurmur).Check());
        }
    }
}
