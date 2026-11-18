using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Several <see cref="ILegBlocker"/>s as one: a leg is blocked when any of them blocks it.</summary>
    public sealed class LegBlockers : ILegBlocker
    {
        private readonly List<ILegBlocker> _blockers = new List<ILegBlocker>(4);

        /// <summary>Adds a blocker.</summary>
        public void Add(ILegBlocker blocker) => _blockers.Add(blocker);

        /// <summary>True when any added blocker is crossed by the walk.</summary>
        public bool Crosses(IReadOnlyList<Vector2> walk)
        {
            for (var i = 0; i < _blockers.Count; i++)
                if (_blockers[i].Crosses(walk)) return true;
            return false;
        }
    }
}
