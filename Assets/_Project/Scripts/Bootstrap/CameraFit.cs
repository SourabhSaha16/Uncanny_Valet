using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Pure zoom choice for the lot camera: close in on the view bounds, but never crop the play area.</summary>
    public static class CameraFit
    {
        /// <summary>
        /// Half the visible height. The view bounds (plus margin) are fitted first and then tightened by
        /// <paramref name="tightness"/> (1 = no tightening); the result never drops below what shows the whole
        /// <paramref name="mustShow"/> rectangle, and is clamped between <paramref name="min"/> and <paramref name="max"/>.
        /// </summary>
        public static float Size(Rect view, Rect mustShow, float aspect, float margin, float tightness, float min, float max)
        {
            var safeAspect = Mathf.Max(0.1f, aspect);
            var fit = Mathf.Max(view.height * 0.5f + margin, (view.width * 0.5f + margin) / safeAspect);
            var floor = Mathf.Max(mustShow.height * 0.5f, mustShow.width * 0.5f / safeAspect);
            var wanted = Mathf.Max(fit * Mathf.Clamp(tightness, 0.1f, 1f), floor);
            return Mathf.Clamp(wanted, min, Mathf.Max(min, max));
        }
    }
}
