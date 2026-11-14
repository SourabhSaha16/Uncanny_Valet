using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>
    /// Base type of the lot mechanics (mud, ice, blizzard, ring, tunnels, pillars, giant cars, shuffles, cliff,
    /// pedestrian collision, ...). Each mechanic is its own asset carrying its own settings; a lot lists the
    /// assets it uses. Read-only at runtime. A mechanic says what it adds to the world in <see cref="Describe"/>
    /// and seeds its tunable numbers in <see cref="SeedInto"/>; the first concrete one is <see cref="MudPatchModifier"/>.
    /// </summary>
    public abstract class LotModifier : ScriptableObject
    {
        [SerializeField, Tooltip("Plain-language name shown in tools and the sandbox.")]
        private string _displayName;

        public string DisplayName => _displayName;

        /// <summary>Adds the world objects this mechanic needs to <paramref name="spec"/>; the scene builder passes the result to the lot builder. Default: none.</summary>
        public virtual void Describe(LotWorldSpec spec) { }

        /// <summary>Seeds this mechanic's tunable numbers into the store (they appear in the F2 panel). Default: none.</summary>
        public virtual void SeedInto(ITuning tuning) { }
    }
}
