using System;
using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>
    /// The layout parameters of a lot that the parametric builder reads: whether the wash slot and the EV bay exist, how much free ground a
    /// corner prop needs under the bays, the wash troughs (separate wash stations outside the bay grid) and how far the lot is widened on the
    /// far side. The default (all zero) is today's diner. Plain data, set by the scene builder from the lot definition.
    /// </summary>
    [Serializable]
    public struct LotLayoutOptions
    {
        /// <summary>Most wash troughs a lot can have.</summary>
        public const int MaxWashTroughs = 3;

        [SerializeField, Tooltip("On: no wash slot and no EV bay; their two grid positions become regular bays (8 regular bays).")]
        private bool _servicesClosed;
        [SerializeField, Min(0f), Tooltip("Free height kept under the bottom bay row, below the usual margin, for a prop in the bottom corner (the Ferris wheel). The lot grows taller when its queue does not already leave this much room.")]
        private float _cornerClearance;
        [SerializeField, Range(0, MaxWashTroughs), Tooltip("Wash troughs: separate wash stations outside the bay grid (needs closed services). With no extension one trough sits beside the bay block; with an extension they stand at the far end.")]
        private int _washTroughs;
        [SerializeField, Min(0f), Tooltip("How much wider the lot is on the far side (the bays stay where they are; the right strip moves out by this much). 0 keeps today's width.")]
        private float _extensionWidth;
        [SerializeField, Min(0f), Tooltip("Gap between the bay block's right edge and the near edge of the troughs at the far end. Only used with an extension.")]
        private float _troughDistance;

        public LotLayoutOptions(bool servicesClosed, float cornerClearance, int washTroughs = 0, float extensionWidth = 0f, float troughDistance = 0f)
        {
            _servicesClosed = servicesClosed;
            _cornerClearance = Mathf.Max(0f, cornerClearance);
            _washTroughs = Mathf.Clamp(washTroughs, 0, MaxWashTroughs);
            _extensionWidth = Mathf.Max(0f, extensionWidth);
            _troughDistance = Mathf.Max(0f, troughDistance);
        }

        /// <summary>True when the lot has no wash slot and no EV bay.</summary>
        public bool ServicesClosed => _servicesClosed;

        /// <summary>Free height reserved under the bottom bay row for a corner prop; 0 reserves nothing.</summary>
        public float CornerClearance => _cornerClearance;

        /// <summary>Wash troughs outside the bay grid (0 to <see cref="MaxWashTroughs"/>).</summary>
        public int WashTroughs => _washTroughs;

        /// <summary>Extra width of the lot on the far side; 0 for none.</summary>
        public float ExtensionWidth => _extensionWidth;

        /// <summary>Gap from the bay block to the troughs at the far end.</summary>
        public float TroughDistance => _troughDistance;
    }
}
