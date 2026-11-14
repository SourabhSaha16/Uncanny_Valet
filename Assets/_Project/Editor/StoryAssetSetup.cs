using System.IO;
using UncannyValet.Core.Story;
using UnityEditor;

namespace UncannyValet.EditorTools
{
    /// <summary>Creates the story text asset (the words of the between-lot cards) from <see cref="StoryTextDrafts"/> when missing. An existing asset is never overwritten, so the owner's edits survive a rebuild.</summary>
    public static class StoryAssetSetup
    {
        public const string StoryTextPath = "Assets/_Project/Data/UI/StoryText.asset";

        public static void CreateIfMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<StoryTextAsset>(StoryTextPath) != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(StoryTextPath));
            var text = StoryTextAsset.Create("StoryText", StoryTextDrafts.Worlds(), StoryTextDrafts.Lots(), StoryTextDrafts.Powers());
            AssetDatabase.CreateAsset(text, StoryTextPath);
        }

        /// <summary>Fills the bootstrap's power definition list (the reward card's names and descriptions) with every power definition asset.</summary>
        public static void AssignPowerDefinitions(SerializedProperty list)
        {
            var guids = AssetDatabase.FindAssets("t:PowerDefinition");
            list.arraySize = guids.Length;
            for (var i = 0; i < guids.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guids[i]));
        }
    }
}
