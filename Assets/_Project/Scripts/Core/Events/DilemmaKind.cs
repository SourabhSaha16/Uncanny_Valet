namespace UncannyValet.Core.Events
{
    /// <summary>The Patron Director's dilemma rules (docs/redesign/patron_director_spec.md, section 4).</summary>
    public enum DilemmaKind
    {
        /// <summary>The wash slot holds a washed car: Dirty is boosted.</summary>
        WashBlocked,

        /// <summary>The wash slot is mid-wash and nothing is queued for it: Dirty is boosted.</summary>
        WashBusy,

        /// <summary>Every EV bay holds a fully charged EV: EV is boosted.</summary>
        BaysBlocked,

        /// <summary>A plain car squats in an EV bay: EV is boosted.</summary>
        BaySquatted,

        /// <summary>Exactly one bay is free and the other is charging: EV is boosted.</summary>
        LastBay,

        /// <summary>Few regular spots are free: Plain is boosted and a return may come forward.</summary>
        LotTight,

        /// <summary>Both stations hold finished cars: Dirty EV is boosted.</summary>
        Chain
    }
}
