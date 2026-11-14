using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Music;
using UncannyValet.Run;
using UncannyValet.Tests.Audio;
using UnityEditor;

namespace UncannyValet.Tests.Run
{
    /// <summary>A lot names its theme and ambience; no theme falls back to the funk loop; the four shipped lots carry their themes.</summary>
    public sealed class LotAudioTests
    {
        private const string Folder = "Assets/_Project/Data/Run/";
        private RunFixtures _run;
        private ThemeFixtures _themes;

        [SetUp]
        public void SetUp()
        {
            _run = new RunFixtures();
            _themes = new ThemeFixtures();
        }

        [TearDown]
        public void TearDown()
        {
            _run.Dispose();
            _themes.Dispose();
            Services.Reset();
        }

        private static LotDefinition Load(string name) => AssetDatabase.LoadAssetAtPath<LotDefinition>(Folder + name + ".asset");

        private static void Give(LotDefinition lot, MusicThemeDefinition theme, AmbienceSetDefinition ambience)
        {
            var so = new SerializedObject(lot);
            so.FindProperty("_musicTheme").objectReferenceValue = theme;
            so.FindProperty("_ambience").objectReferenceValue = ambience;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        [Test]
        public void ALotWithoutAThemeRegistersNoThemeSoTheFunkLoopStays()
        {
            var lot = _run.Lot(World.Midgard, LotKind.Training);
            Assert.IsNull(lot.MusicTheme);
            AudioBootstrap.Register(lot);
            Assert.IsNull(Services.Get<ILotAudio>().Theme);
            Assert.IsNull(Services.Get<ILotAudio>().Ambience);
        }

        [Test]
        public void ALotHandsItsThemeAndAmbienceToTheAudioModule()
        {
            var lot = _run.Lot(World.Vanaheim, LotKind.Crazy);
            var theme = _themes.Theme(voices: ThemeVoice.Stomp);
            var ambience = _themes.Ambience(AmbienceKind.DrippingEaves);
            Give(lot, theme, ambience);
            Assert.IsTrue(lot.TryValidate(out var problem), problem);
            AudioBootstrap.Register(lot);
            Assert.AreSame(theme, Services.Get<ILotAudio>().Theme);
            Assert.AreSame(ambience, Services.Get<ILotAudio>().Ambience);
        }

        [Test]
        public void ALotWithABrokenThemeFailsValidation()
        {
            var lot = _run.Lot(World.Vanaheim, LotKind.Crazy);
            Give(lot, _themes.Theme(bpm: 5f, voices: ThemeVoice.Stomp), null);
            Assert.IsFalse(lot.TryValidate(out var problem));
            StringAssert.Contains("tempo", problem);
        }

        [TestCase("MidgardTrainingLot", "MidgardCrazyLot")]
        [TestCase("VanaheimTrainingLot", "VanaheimCrazyLot")]
        public void EachWorldPlaysItsTuneFasterAndRowdierInTheCrazyLot(string training, string crazy)
        {
            var calm = Load(training).MusicTheme;
            var rowdy = Load(crazy).MusicTheme;
            Assert.IsNotNull(calm, training);
            Assert.IsNotNull(rowdy, crazy);
            Assert.IsTrue(calm.TryValidate(out var problem), problem);
            Assert.IsTrue(rowdy.TryValidate(out problem), problem);
            Assert.Greater(rowdy.TempoBpm, calm.TempoBpm);
            Assert.Greater(rowdy.Voices.Count, calm.Voices.Count);
            CollectionAssert.AreEqual((System.Collections.IEnumerable)calm.Melody, (System.Collections.IEnumerable)rowdy.Melody, "the same tune");
            Assert.AreEqual(0.15f, calm.EscalationTempoRise, 1e-5f);
            Assert.AreEqual(0.15f, rowdy.EscalationTempoRise, 1e-5f);
        }

        [Test]
        public void TheShippedThemesUseTheInstrumentsOfTheDesign()
        {
            AssertVoices("MidgardTrainingLot", ThemeVoice.FolkGuitar, ThemeVoice.Shaker);
            AssertVoices("MidgardCrazyLot", ThemeVoice.FolkGuitar, ThemeVoice.Shaker, ThemeVoice.FairOrgan);
            AssertVoices("VanaheimTrainingLot", ThemeVoice.FiddleSaw, ThemeVoice.WoodenFlute, ThemeVoice.FrameDrum);
            AssertVoices("VanaheimCrazyLot", ThemeVoice.FiddleSaw, ThemeVoice.WoodenFlute, ThemeVoice.FrameDrum, ThemeVoice.Stomp, ThemeVoice.Clap);
            Assert.AreEqual(EscalationLayer.Fireworks, Load("MidgardCrazyLot").MusicTheme.EscalationLayer);
            Assert.AreEqual(EscalationLayer.ThunderCloser, Load("VanaheimCrazyLot").MusicTheme.EscalationLayer);
        }

        [Test]
        public void TheShippedAmbienceMatchesTheDesign()
        {
            Assert.IsNull(Load("MidgardTrainingLot").Ambience);
            var fair = Load("MidgardCrazyLot").Ambience;
            Assert.IsTrue(fair.Has(AmbienceKind.CrowdMurmur) && fair.Has(AmbienceKind.DistantRides));
            var calm = Load("VanaheimTrainingLot").Ambience;
            Assert.IsTrue(calm.Has(AmbienceKind.WaterwheelCreak) && calm.Has(AmbienceKind.WashSplash));
            Assert.IsFalse(calm.Has(AmbienceKind.DrippingEaves));
            var storm = Load("VanaheimCrazyLot").Ambience;
            Assert.IsTrue(storm.Has(AmbienceKind.DrippingEaves) && storm.Has(AmbienceKind.DistantThunder) && storm.Has(AmbienceKind.WaterwheelCreak));
        }

        private static void AssertVoices(string lot, params ThemeVoice[] expected)
        {
            var actual = new System.Collections.Generic.List<ThemeVoice>();
            foreach (var mix in Load(lot).MusicTheme.Voices) actual.Add(mix.Voice);
            CollectionAssert.AreEquivalent(expected, actual, lot);
        }
    }
}
