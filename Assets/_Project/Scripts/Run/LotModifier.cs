using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>
    /// Base type of the lot mechanics (mud, ice, blizzard, ring, tunnels, pillars, giant cars, shuffles, cliff,
    /// pedestrian collision, ...). Each mechanic is its own asset carrying its own settings; a lot lists the
    /// assets it uses. Read-only at runtime. No concrete mechanic exists yet.
    /// </summary>
    public abstract class LotModifier : ScriptableObject
    {
        [SerializeField, Tooltip("Plain-language name shown in tools and the sandbox.")]
        private string _displayName;

        public string DisplayName => _displayName;
    }
}
