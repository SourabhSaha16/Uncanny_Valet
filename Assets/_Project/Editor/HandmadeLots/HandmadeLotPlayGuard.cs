using UncannyValet.Lot.Handmade;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>Stops Play mode from starting while an open hand-made lot has a validator error, and says why in the console.</summary>
    [InitializeOnLoad]
    public static class HandmadeLotPlayGuard
    {
        static HandmadeLotPlayGuard() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            foreach (var lot in LotValidation.FindLots())
            {
                var issues = LotValidation.Check(lot);
                if (!HandmadeLotRules.HasErrors(issues)) continue;
                EditorApplication.isPlaying = false;
                Debug.LogError("Hand-made lot " + lot.name + " cannot play: " + LotValidation.ErrorSummary(issues), lot);
                return;
            }
        }
    }
}
