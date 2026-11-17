using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>Nothing of the previous lot keeps sounding after a lot change, and a finished lot silences every layer (volume and clip state; batch mode may play nothing).</summary>
    public sealed class LevelAudioResetPlayModeTests : VanaheimPlayBase
    {
        private const string MidgardCrazyPath = "Assets/_Project/Scenes/Lot_MidgardCrazy.unity";

        [UnityTest]
        public IEnumerator ALotChangeLeavesOnlyTheNewLotsLoops()
        {
            yield return Play(MidgardCrazyPath);
            yield return null;
            Assert.IsTrue(AudibleClips().Contains("MidgardCrazyTheme"), "the first lot plays its theme");
            yield return Play(TrainingPath);
            yield return null;
            var clips = AudibleClips();
            Assert.IsTrue(clips.Contains("VanaheimTrainingTheme"), "the new lot plays its theme");
            Assert.IsFalse(clips.Contains("MidgardCrazyTheme"), "the previous theme is gone");
            Assert.IsFalse(clips.Contains("CrowdMurmur") || clips.Contains("DistantRides"), "the previous ambience is gone");
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }

        [UnityTest]
        public IEnumerator AFinishedLotSilencesEveryLayer()
        {
            yield return Play(MidgardCrazyPath);
            yield return null;
            Services.Get<IEventBus>().Publish(new NextLotChosen(default(LotId), RunOutcome.Advanced));
            yield return null;
            Assert.IsEmpty(AudibleClips(), "no loop keeps a clip or a volume");
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }

        // Names of the looping clips that could be heard: assigned to a loop source with volume above zero.
        private static List<string> AudibleClips()
        {
            var names = new List<string>();
            foreach (var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                if (source.loop && source.clip != null && source.volume > 0f && source.clip.name != "SquealLoop") names.Add(source.clip.name);
            return names;
        }
    }
}
