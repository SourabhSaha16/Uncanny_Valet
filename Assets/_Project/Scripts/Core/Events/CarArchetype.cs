namespace UncannyValet.Core.Events
{
    /// <summary>The kind of patron car the director can send (docs/redesign/patron_director_spec.md, section 4).</summary>
    public enum CarArchetype
    {
        /// <summary>Park, return.</summary>
        Plain,

        /// <summary>Muddy: wash, then park.</summary>
        Muddy,

        /// <summary>Charge before pickup for a bonus.</summary>
        Ev,

        /// <summary>Muddy and needs a charge: wash and charge: two stations in sequence.</summary>
        MuddyEv
    }
}
