using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Music;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Each Midgard and Vanaheim lot starts its own theme and ambience without errors (the audio director's state is checked; batch mode may have no audio device).</summary>
    public sealed class ThemedLotAudioPlayModeTests : VanaheimPlayBase
    {
        private const string MidgardTrainingPath = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string MidgardCrazyPath = "Assets/_Project/Scenes/Lot_MidgardCrazy.unity";

        [UnityTest]
        public IEnumerator MidgardTrainingPlaysTheLazyAcousticTheme() { yield return Check(MidgardTrainingPath, "MidgardTrainingTheme", 0); }

        [UnityTest]
        public IEnumerator MidgardCrazyPlaysTheFairNightThemeWithItsAmbience() { yield return Check(MidgardCrazyPath, "MidgardCrazyTheme", 2); }

        [UnityTest]
        public IEnumerator VanaheimTrainingPlaysTheCalmFolkTheme() { yield return Check(TrainingPath, "VanaheimTrainingTheme", 1); }

        [UnityTest]
        public IEnumerator VanaheimCrazyPlaysTheHarvestFeastThemeWithItsAmbience() { yield return Check(CrazyPath, "VanaheimCrazyTheme", 3); }

        private IEnumerator Check(string path, string themeName, int loopLayers)
        {
            yield return Play(path);
            yield return null;
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
            var lotAudio = Services.Get<ILotAudio>();
            Assert.AreEqual(themeName, lotAudio.Theme.name);
            var director = Object.FindFirstObjectByType<AudioDirector>();
            Assert.IsNotNull(director);
            var theme = (MusicThemeDefinition)Field(director, "_theme");
            Assert.AreSame(lotAudio.Theme, theme, "the director plays the lot's theme");
            var music = (AudioSource)Field(director, "_music");
            Assert.AreEqual(themeName, music.clip.name, "the music clip is the theme, not the funk loop");
            var session = (ThemeSession)Field(director, "_session");
            Assert.IsNotNull(session);
            Assert.AreEqual(loopLayers, LoopSources(director), "ambience loops and the escalation layer");
        }

        // The escalation layer plus one looping source per ambience layer (the wash splash is a one-shot).
        private static int LoopSources(AudioDirector director)
        {
            var count = 0;
            foreach (var source in director.GetComponents<AudioSource>())
                if (source.loop && source.clip != null && source.clip.name != "FunkLoop" && !source.clip.name.EndsWith("Theme") && source.clip.name != "SquealLoop") count++;
            return count - 1; // minus the escalation layer
        }

        private static object Field(AudioDirector director, string name)
            => typeof(AudioDirector).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(director);
    }
}
