using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Something a roaming leg must not run into (a keep-out rectangle, a parked car). New kinds are new classes behind this.</summary>
    public interface ILegBlocker
    {
        /// <summary>True when any straight piece of the walk touches the obstacle.</summary>
        bool Crosses(IReadOnlyList<Vector2> walk);
    }
}
