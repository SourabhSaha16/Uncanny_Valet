using System.IO;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Music;
using UncannyValet.Run;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Creates the first-pass music themes (Midgard and Vanaheim, training and crazy) and ambience sets, and gives them to the lots that have none.
    /// Existing assets are never overwritten, so the owner's tuning survives a rebuild.
    /// </summary>
    public static class AudioThemeAssets
    {
        public const string Folder = "Assets/_Project/Data/Audio/Themes";
        private const int R = MusicThemeDefinition.Rest;
        private const int MidgardRoot = 57;
        private const int VanaheimRoot = 55;
        private const float FireworksRamp = 1.5f;
        private const float ThunderRamp = 25f;

        private static readonly int[] Chords = { 0, 5, 0, 7 };

        // One tune per world; the crazy lot plays it faster with more instruments.
        private static readonly int[] MidgardTune =
        {
            4, R, 7, R, 9, R, 7, R, 9, R, R, R, 7, R, 9, R,
            12, R, R, R, 9, R, 7, R, 7, R, R, R, 4, R, R, R
        };
        private static readonly int[] VanaheimTune =
        {
            7, R, R, 10, 12, R, 10, R, 12, R, R, R, 10, R, 7, R,
            7, R, R, 5, 7, R, 3, R, 5, R, R, R, 7, R, R, R
        };

        public static void CreateIfMissing()
        {
            Directory.CreateDirectory(Folder);
            var midgardTraining = Theme("MidgardTrainingTheme", 84f, 0.12f, MidgardRoot, false, MidgardTune, Chords, EscalationLayer.Fireworks, FireworksRamp,
                V(ThemeVoice.FolkGuitar, 0.9f), V(ThemeVoice.Shaker, 0.5f));
            var midgardCrazy = Theme("MidgardCrazyTheme", 118f, 0.05f, MidgardRoot, false, MidgardTune, Chords, EscalationLayer.Fireworks, FireworksRamp,
                V(ThemeVoice.FolkGuitar, 0.6f), V(ThemeVoice.Shaker, 0.6f), V(ThemeVoice.FairOrgan, 0.9f));
            var vanaheimTraining = Theme("VanaheimTrainingTheme", 76f, 0.1f, VanaheimRoot, true, VanaheimTune, Chords, EscalationLayer.ThunderCloser, ThunderRamp,
                V(ThemeVoice.FiddleSaw, 0.9f), V(ThemeVoice.WoodenFlute, 0.6f), V(ThemeVoice.FrameDrum, 0.6f));
            var vanaheimCrazy = Theme("VanaheimCrazyTheme", 108f, 0.06f, VanaheimRoot, true, VanaheimTune, Chords, EscalationLayer.ThunderCloser, ThunderRamp,
                V(ThemeVoice.FiddleSaw, 0.9f), V(ThemeVoice.WoodenFlute, 0.5f), V(ThemeVoice.FrameDrum, 0.5f), V(ThemeVoice.Stomp, 0.8f), V(ThemeVoice.Clap, 0.7f));
            var midgardFair = Ambience("MidgardCrazyAmbience", L(AmbienceKind.CrowdMurmur, 0.7f), L(AmbienceKind.DistantRides, 0.5f));
            var vanaheimCalm = Ambience("VanaheimTrainingAmbience", L(AmbienceKind.WaterwheelCreak, 0.6f), L(AmbienceKind.WashSplash, 1f));
            var vanaheimStorm = Ambience("VanaheimCrazyAmbience", L(AmbienceKind.WaterwheelCreak, 0.5f), L(AmbienceKind.DrippingEaves, 0.6f),
                L(AmbienceKind.DistantThunder, 0.6f), L(AmbienceKind.WashSplash, 1f));
            Assign(RunAssetSetup.MidgardTrainingPath, midgardTraining, null);
            Assign(RunAssetSetup.MidgardCrazyPath, midgardCrazy, midgardFair);
            Assign(RunAssetSetup.VanaheimTrainingPath, vanaheimTraining, vanaheimCalm);
            Assign(RunAssetSetup.VanaheimCrazyPath, vanaheimCrazy, vanaheimStorm);
            AssetDatabase.SaveAssets();
        }

        private static ThemeVoiceMix V(ThemeVoice voice, float volume) => new ThemeVoiceMix(voice, volume);

        private static AmbienceLayerMix L(AmbienceKind kind, float level) => new AmbienceLayerMix(kind, level);

        private static MusicThemeDefinition Theme(string name, float bpm, float swing, int root, bool minor, int[] melody, int[] chords,
            EscalationLayer layer, float ramp, params ThemeVoiceMix[] voices)
        {
            var path = Folder + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<MusicThemeDefinition>(path);
            if (existing != null) return existing;
            var theme = ScriptableObject.CreateInstance<MusicThemeDefinition>();
            AssetDatabase.CreateAsset(theme, path);
            var so = new SerializedObject(theme);
            so.FindProperty("_tempoBpm").floatValue = bpm;
            so.FindProperty("_swing").floatValue = swing;
            so.FindProperty("_rootMidiNote").intValue = root;
            so.FindProperty("_minor").boolValue = minor;
            SetInts(so.FindProperty("_melody"), melody);
            SetInts(so.FindProperty("_chordRoots"), chords);
            var list = so.FindProperty("_voices");
            list.arraySize = voices.Length;
            for (var i = 0; i < voices.Length; i++)
            {
                var item = list.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("_voice").intValue = (int)voices[i].Voice;
                item.FindPropertyRelative("_volume").floatValue = voices[i].Volume;
            }
            so.FindProperty("_escalationLayer").intValue = (int)layer;
            so.FindProperty("_escalationRampSeconds").floatValue = ramp;
            so.ApplyModifiedPropertiesWithoutUndo();
            return theme;
        }

        private static AmbienceSetDefinition Ambience(string name, params AmbienceLayerMix[] layers)
        {
            var path = Folder + "/" + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<AmbienceSetDefinition>(path);
            if (existing != null) return existing;
            var set = ScriptableObject.CreateInstance<AmbienceSetDefinition>();
            AssetDatabase.CreateAsset(set, path);
            var so = new SerializedObject(set);
            var list = so.FindProperty("_layers");
            list.arraySize = layers.Length;
            for (var i = 0; i < layers.Length; i++)
            {
                var item = list.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("_kind").intValue = (int)layers[i].Kind;
                item.FindPropertyRelative("_level").floatValue = layers[i].Level;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return set;
        }

        private static void SetInts(SerializedProperty array, int[] values)
        {
            array.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).intValue = values[i];
        }

        // A lot that already names a theme keeps it.
        private static void Assign(string lotPath, MusicThemeDefinition theme, AmbienceSetDefinition ambience)
        {
            var lot = AssetDatabase.LoadAssetAtPath<LotDefinition>(lotPath);
            if (lot == null || lot.MusicTheme != null) return;
            var so = new SerializedObject(lot);
            so.FindProperty("_musicTheme").objectReferenceValue = theme;
            so.FindProperty("_ambience").objectReferenceValue = ambience;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(lot);
        }
    }
}
