using System;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The layout parameters of a lot that the parametric builder reads: whether the wash slot and the EV bay exist, and how much free ground a
    /// corner prop needs under the bays. The default (all zero) is today's diner. Plain data, set by the scene builder from the lot definition.
    /// </summary>
    [Serializable]
    public struct LotLayoutOptions
    {
        [SerializeField, Tooltip("On: no wash slot and no EV bay; their two grid positions become regular bays (8 regular bays).")]
        private bool _servicesClosed;
        [SerializeField, Min(0f), Tooltip("Free height kept under the bottom bay row, below the usual margin, for a prop in the bottom corner (the Ferris wheel). The lot grows taller when its queue does not already leave this much room.")]
        private float _cornerClearance;

        public LotLayoutOptions(bool servicesClosed, float cornerClearance)
        {
            _servicesClosed = servicesClosed;
            _cornerClearance = Mathf.Max(0f, cornerClearance);
        }

        /// <summary>True when the lot has no wash slot and no EV bay.</summary>
        public bool ServicesClosed => _servicesClosed;

        /// <summary>Free height reserved under the bottom bay row for a corner prop; 0 reserves nothing.</summary>
        public float CornerClearance => _cornerClearance;
    }
}
