using System;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Tunables for the crosswalk zebra bars, the arrival slot boxes and the curb standing spots. World units.</summary>
    [Serializable]
    public sealed class WalkwayStyle
    {
        [Header("Crosswalk zebra bars")]
        [SerializeField, Min(0.05f), Tooltip("Length of one zebra bar along the walking direction.")]
        private float _zebraBar = 0.3f;
        [SerializeField, Min(0.05f), Tooltip("Gap between two zebra bars.")]
        private float _zebraGap = 0.3f;
        [SerializeField, Min(0f), Tooltip("How far each zebra bar stops short of the crosswalk edge, on both sides.")]
        private float _zebraMargin = 0.05f;
        [Header("Arrival queue slots (dotted boxes)")]
        [SerializeField, Min(0.02f), Tooltip("Line thickness of the dotted box around each arrival slot.")]
        private float _slotLine = 0.1f;
        [SerializeField, Min(0.05f), Tooltip("Length of one dash of the dotted slot box.")]
        private float _slotDash = 0.4f;
        [SerializeField, Min(0.02f), Tooltip("Gap between two dashes of the dotted slot box.")]
        private float _slotGap = 0.25f;
        [SerializeField, Min(0f), Tooltip("How far the dotted box sits inside the slot's car footprint, so neighbouring boxes do not touch.")]
        private float _slotInset = 0.12f;
        [Header("Curb standing spots (in the gap west of the curb zone)")]
        [SerializeField, Min(0.1f), Tooltip("Spacing between curb standing spots along the curb.")]
        private float _waitSpacing = 1.6f;
        [SerializeField, Range(1, 5), Tooltip("Number of curb standing spots.")]
        private int _waitSlots = 3;

        public float ZebraBar => _zebraBar;
        public float ZebraGap => _zebraGap;
        public float ZebraMargin => _zebraMargin;
        public float SlotLine => _slotLine;
        public float SlotDash => _slotDash;
        public float SlotGap => _slotGap;
        public float SlotInset => _slotInset;
        public float WaitSpacing => _waitSpacing;
        public int WaitSlots => _waitSlots;
    }
}
