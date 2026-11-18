using System.Collections.Generic;
using UncannyValet.Lot.Handmade;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Finds the hand-made lots being edited and runs <see cref="HandmadeLotRules"/> on them: the one place the validator window and the play guard share.</summary>
    public static class LotValidation
    {
        /// <summary>The lots of the open prefab stage, or of every loaded scene when no prefab is open.</summary>
        public static List<HandmadeLot> FindLots()
        {
            var lots = new List<HandmadeLot>();
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                lots.AddRange(stage.prefabContentsRoot.GetComponentsInChildren<HandmadeLot>(true));
                return lots;
            }
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects()) lots.AddRange(root.GetComponentsInChildren<HandmadeLot>(true));
            }
            return lots;
        }

        /// <summary>Reads the markers under <paramref name="lot"/> and returns every finding (errors and warnings).</summary>
        public static List<LotIssue> Check(HandmadeLot lot)
        {
            var issues = new List<LotIssue>();
            HandmadeLotRules.Validate(HandmadeLotReader.Read(lot), issues);
            return issues;
        }

        /// <summary>The marker an issue is about: the first object under the lot with the issue's subject name, or the lot itself.</summary>
        public static GameObject Find(HandmadeLot lot, LotIssue issue)
        {
            if (issue.Subject.Length == 0) return lot.gameObject;
            foreach (var child in lot.GetComponentsInChildren<Transform>(true))
                if (child.name == issue.Subject) return child.gameObject;
            return lot.gameObject;
        }

        /// <summary>The errors as one line, for log messages.</summary>
        public static string ErrorSummary(IReadOnlyList<LotIssue> issues)
        {
            var parts = new List<string>();
            foreach (var issue in issues)
                if (issue.IsError) parts.Add(issue.ToString());
            return string.Join("; ", parts);
        }
    }
}
