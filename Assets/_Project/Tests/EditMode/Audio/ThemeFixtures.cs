using System.Collections.Generic;
using UncannyValet.Core.Music;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.Tests.Audio
{
    /// <summary>Builds in-memory themes and ambience sets for the audio tests; destroys them on <see cref="Dispose"/>.</summary>
    public sealed class ThemeFixtures : System.IDisposable
    {
        private const int R = MusicThemeDefinition.Rest;
        private readonly List<Object> _made = new List<Object>();

        public static readonly int[] Tune =
        {
            0, R, 4, R, 7, R, 4, R, 2, R, R, R, 5, R, 7, R,
            9, R, R, R, 7, R, 4, R, 2, R, R, R, 0, R, R, R
        };

        public MusicThemeDefinition Theme(float bpm = 90f, bool minor = false, EscalationLayer layer = EscalationLayer.Fireworks,
            AudioClip clip = null, params ThemeVoice[] voices)
        {
            var theme = Make<MusicThemeDefinition>("Theme");
            var so = new SerializedObject(theme);
            so.FindProperty("_tempoBpm").floatValue = bpm;
            so.FindProperty("_minor").boolValue = minor;
            var melody = so.FindProperty("_melody");
            melody.arraySize = Tune.Length;
            for (var i = 0; i < Tune.Length; i++) melody.GetArrayElementAtIndex(i).intValue = Tune[i];
            var chords = so.FindProperty("_chordRoots");
            chords.arraySize = MusicThemeDefinition.ChordCount;
            var roots = new[] { 0, 5, 0, 7 };
            for (var i = 0; i < roots.Length; i++) chords.GetArrayElementAtIndex(i).intValue = roots[i];
            var list = so.FindProperty("_voices");
            list.arraySize = voices.Length;
            for (var i = 0; i < voices.Length; i++)
            {
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_voice").intValue = (int)voices[i];
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_volume").floatValue = 0.8f;
            }
            so.FindProperty("_escalationLayer").intValue = (int)layer;
            so.FindProperty("_clipOverride").objectReferenceValue = clip;
            so.ApplyModifiedPropertiesWithoutUndo();
            return theme;
        }

        public AmbienceSetDefinition Ambience(params AmbienceKind[] kinds)
        {
            var set = Make<AmbienceSetDefinition>("Ambience");
            var so = new SerializedObject(set);
            var list = so.FindProperty("_layers");
            list.arraySize = kinds.Length;
            for (var i = 0; i < kinds.Length; i++)
            {
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_kind").intValue = (int)kinds[i];
                list.GetArrayElementAtIndex(i).FindPropertyRelative("_level").floatValue = 0.5f;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return set;
        }

        public AudioClip Clip()
        {
            var clip = AudioClip.Create("Recording", 100, 1, 22050, false);
            _made.Add(clip);
            return clip;
        }

        public T Make<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            _made.Add(asset);
            return asset;
        }

        public void Dispose()
        {
            foreach (var asset in _made) if (asset != null) Object.DestroyImmediate(asset);
        }
    }
}
