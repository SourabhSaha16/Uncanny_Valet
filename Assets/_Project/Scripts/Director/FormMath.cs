using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>Pure maths for form F: the instantaneous target and the limited step toward it.</summary>
    public static class FormMath
    {
        public const float MinForm = -1f;
        public const float MaxForm = 1f;

        /// <summary>
        /// The form the player's recent play points at, -1..1. The tip term is the tip rate against the 3-star
        /// pace (-1 at no tips, 0 on pace, +1 at double pace); angry patrons pull down; a combo pushes up.
        /// </summary>
        public static float Target(float tipsPerSecond, float paceTipsPerSecond, int angryRecent, int combo,
            int maxCombo, FormSettings s)
        {
            var tipTerm = paceTipsPerSecond > 0f ? Mathf.Clamp(tipsPerSecond / paceTipsPerSecond - 1f, MinForm, MaxForm) : 0f;
            var angryTerm = -Mathf.Clamp01(angryRecent / (float)s.AngryForFullPenalty);
            var comboTerm = maxCombo > 1 ? Mathf.Clamp01((combo - 1) / (float)(maxCombo - 1)) : 0f;
            var sum = s.TipWeight * tipTerm + s.AngryWeight * angryTerm + s.ComboWeight * comboTerm;
            return Mathf.Clamp(sum, MinForm, MaxForm);
        }

        /// <summary>Moves <paramref name="current"/> toward <paramref name="target"/> by at most <paramref name="maxStep"/>, within -1..1.</summary>
        public static float Step(float current, float target, float maxStep)
        {
            var moved = current + Mathf.Clamp(target - current, -maxStep, maxStep);
            return Mathf.Clamp(moved, MinForm, MaxForm);
        }
    }
}
