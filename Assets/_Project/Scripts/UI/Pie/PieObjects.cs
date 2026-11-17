using UnityEngine;

namespace UncannyValet.UI.Pie
{
    /// <summary>Releases the pie's Unity objects: deferred while playing, immediate in edit mode (where deferred destroy is an error).</summary>
    internal static class PieObjects
    {
        public static void Release(Object target)
        {
            if (target == null) return;
            if (Application.isPlaying) Object.Destroy(target);
            else Object.DestroyImmediate(target);
        }
    }
}
