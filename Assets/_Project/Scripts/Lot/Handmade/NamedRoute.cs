using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>A marker that is a polyline in travel order: an ambient lane.</summary>
    public readonly struct NamedRoute
    {
        public readonly string Name;
        public readonly Vector2[] Points;

        public NamedRoute(string name, Vector2[] points)
        {
            Name = name;
            Points = points;
        }
    }
}
