using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows, default values and seeding of the pedestrian hit: knockback, car slowdown and the patron curse.</summary>
    public static class PedestrianHitRows
    {
        public const string Group = "Pedestrian hits";
        public const float NearDistance = 0.7f;
        public const float FarDistance = 4f;
        public const float NearSpeed = 1f;
        public const float FarSpeed = 8f;
        public const float NearTumble = 0.4f;
        public const float FarTumble = 0.8f;
        public const float CarSlowdown = 0.35f;
        public const float CarSlowFloor = 0.3f;
        public const float CurseAmount = 5f;
        public const float CurseLockout = 3f;
        public const float SlightCurseAmount = 2f;

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.PedestrianKnockbackNear, Group, "Knockback: distance at a gentle hit (units)", 0f, 6f, 0.1f,
                "How far a person is thrown when a car bumps them at walking pace."),
            new TuningInfo(TuningKey.PedestrianKnockbackFar, Group, "Knockback: distance at a full-speed hit (units)", 0f, 8f, 0.1f,
                "How far a person is thrown by a car at its top speed. Faster hits never throw further."),
            new TuningInfo(TuningKey.PedestrianKnockbackNearSpeed, Group, "Knockback: the gentle hit speed (units/s)", 0.1f, 8f, 0.1f,
                "Hit speed that gives the gentle distance and tumble."),
            new TuningInfo(TuningKey.PedestrianKnockbackFarSpeed, Group, "Knockback: the full-speed hit speed (units/s)", 1f, 20f, 0.5f,
                "Hit speed that gives the full distance and tumble (about the car's top speed)."),
            new TuningInfo(TuningKey.PedestrianTumbleNear, Group, "Tumble: seconds at a gentle hit", 0.1f, 2f, 0.05f,
                "How long a person spins on the ground after a gentle hit before getting up."),
            new TuningInfo(TuningKey.PedestrianTumbleFar, Group, "Tumble: seconds at a full-speed hit", 0.1f, 2f, 0.05f,
                "How long a person spins after a full-speed hit."),
            new TuningInfo(TuningKey.PedestrianCarSlowdown, Group, "Car slowdown when it hits a person (share)", 0f, 0.8f, 0.01f,
                "Share of its speed the car loses once per hit. 0.35 is 35%. The car is never bounced back."),
            new TuningInfo(TuningKey.PedestrianCarSlowFloor, Group, "Car slowdown: never below (share of top speed)", 0f, 1f, 0.05f,
                "Hitting people never slows the car below this share of its top speed."),
            new TuningInfo(TuningKey.PedestrianCurseAmount, Group, "Curse for hitting a patron (blessings)", 0f, 50f, 1f,
                "Blessings taken off when the car you drive hits a patron on foot. Favour resets too."),
            new TuningInfo(TuningKey.PedestrianSlightCurseAmount, Group, "Slight curse for hitting any other person (blessings)", 0f, 50f, 1f,
                "Blessings taken off when the car you drive hits a passer-by (not a patron the full curse covers). Every lot, ignores the curse switch. Same lockout as the patron curse."),
            new TuningInfo(TuningKey.PedestrianCurseLockout, Group, "Curse: seconds before the same patron can curse again", 0f, 20f, 0.5f,
                "Hitting the same patron again within this time costs nothing."),
            new TuningInfo(TuningKey.PedestrianCurses, Group, "Hitting patrons is a curse (1 yes, 0 no)", 0f, 1f, 1f,
                "Set per lot: on in crazy lots, off in training lots. Knockback, slowdown and the yelp always happen."),
            new TuningInfo(TuningKey.PedestrianFreeRoutes, Group, "Pedestrians walk any direct route (1 yes, 0 crosswalks only)", 0f, 1f, 1f,
                "Set per lot: on in crazy lots so people never queue at the crosswalk, off in training lots. Applies to the next walk."),
        };

        /// <summary>Seeds every key with its default; <paramref name="cursesOn"/> and <paramref name="freeRoutes"/> are the lot's switches.</summary>
        public static void Seed(ITuning t, bool cursesOn, bool freeRoutes = false)
        {
            t.Seed(TuningKey.PedestrianKnockbackNear, NearDistance);
            t.Seed(TuningKey.PedestrianKnockbackFar, FarDistance);
            t.Seed(TuningKey.PedestrianKnockbackNearSpeed, NearSpeed);
            t.Seed(TuningKey.PedestrianKnockbackFarSpeed, FarSpeed);
            t.Seed(TuningKey.PedestrianTumbleNear, NearTumble);
            t.Seed(TuningKey.PedestrianTumbleFar, FarTumble);
            t.Seed(TuningKey.PedestrianCarSlowdown, CarSlowdown);
            t.Seed(TuningKey.PedestrianCarSlowFloor, CarSlowFloor);
            t.Seed(TuningKey.PedestrianCurseAmount, CurseAmount);
            t.Seed(TuningKey.PedestrianCurseLockout, CurseLockout);
            t.Seed(TuningKey.PedestrianSlightCurseAmount, SlightCurseAmount);
            t.Seed(TuningKey.PedestrianCurses, cursesOn ? 1f : 0f);
            t.Seed(TuningKey.PedestrianFreeRoutes, freeRoutes ? 1f : 0f);
        }
    }
}
