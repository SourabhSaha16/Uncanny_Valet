namespace UncannyValet.Core.Events
{
    /// <summary>The kind of patron car the director can send (docs/redesign/patron_director_spec.md, section 4).</summary>
    public enum CarArchetype
    {
        /// <summary>Park, return.</summary>
        Plain,

        /// <summary>Wash, then park.</summary>
        Dirty,

        /// <summary>Charge before pickup for a bonus.</summary>
        Ev,

        /// <summary>Wash and charge: two stations in sequence.</summary>
        DirtyEv
    }
}
