namespace UncannyValet.Core
{
    /// <summary>Physics layer indices shared between modules. The names are set in ProjectSettings/TagManager.</summary>
    public static class PhysicsLayers
    {
        /// <summary>Layer of every pedestrian collider (ambient people and patrons on foot).</summary>
        public const int Pedestrian = 8;

        /// <summary>Layer mask of <see cref="Pedestrian"/>.</summary>
        public const int PedestrianMask = 1 << Pedestrian;
    }
}
