using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>Pure maths for form F: the instantaneous target and the limited step toward it.</summary>
    public static class FormMath
    {
        public const float MinForm = -1f;
        public const float MaxForm = 1f;

        /// <summary>
        /// The form the player's recent play points at, -1..1. The blessing term is the blessing rate against the pace target
        /// pace (-1 at no blessings, 0 on pace, +1 at double pace); angry patrons pull down; a favour pushes up.
        /// </summary>
        public static float Target(float blessingsPerSecond, float paceBlessingsPerSecond, int angryRecent, int favour,
            int maxFavour, FormSettings s)
        {
            var blessingTerm = paceBlessingsPerSecond > 0f ? Mathf.Clamp(blessingsPerSecond / paceBlessingsPerSecond - 1f, MinForm, MaxForm) : 0f;
            var angryTerm = -Mathf.Clamp01(angryRecent / (float)s.AngryForFullPenalty);
            var favourTerm = maxFavour > 1 ? Mathf.Clamp01((favour - 1) / (float)(maxFavour - 1)) : 0f;
            var sum = s.BlessingWeight * blessingTerm + s.AngryWeight * angryTerm + s.FavourWeight * favourTerm;
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
