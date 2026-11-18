using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>The car exit road: the lane (travel order, ending off screen) and the route from inside the curb zone to the merge point on that lane (its last point).</summary>
    public readonly struct ExitRoadData
    {
        public readonly string Name;
        public readonly Vector2[] Lane;
        public readonly Vector2[] CurbExit;

        public ExitRoadData(string name, Vector2[] lane, Vector2[] curbExit)
        {
            Name = name;
            Lane = lane;
            CurbExit = curbExit;
        }
    }
}
