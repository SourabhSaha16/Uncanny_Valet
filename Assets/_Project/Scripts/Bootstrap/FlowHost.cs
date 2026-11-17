using System;
using UncannyValet.Core.Ids;
using UncannyValet.UI.Flow;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Keeps the story flow's state across lot scenes: it sits on the persistent run host's object (so it is created and destroyed with it) and
    /// owns the one <see cref="FlowMachine"/>. Every lot scene's UI picks the machine up from it.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FlowHost : MonoBehaviour
    {
        /// <summary>The flow state shared by every lot scene.</summary>
        public FlowMachine Machine { get; private set; }

        /// <summary>Creates the machine; call once, right after adding the component.</summary>
        /// <param name="startLot">The lot the story starts at, asked when the title is dismissed.</param>
        public void Initialize(Func<LotId> startLot)
        {
            Machine = new FlowMachine(startLot);
        }
    }
}
