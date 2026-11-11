using System;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Tunables for the lot site around the bays (lot v2): the diner and its doors, the right strip, the walkways, the road below. World units.</summary>
    [Serializable]
    public sealed class SiteLayout
    {
        [Header("Diner")]
        [SerializeField, Min(0.5f), Tooltip("Open gap between the diner's right wall and the curb zone. Waiting patrons stand in it and face the curb zone.")]
        private float _curbGap = 1.75f;
        [SerializeField, Min(2f), Tooltip("Width of the diner, which fills the left side of the lot up to the curb gap.")]
        private float _dinerWidth = 8f;
        [SerializeField, Min(0f), Tooltip("Gap between the diner and the top and bottom walls.")]
        private float _dinerInset = 1f;
        [SerializeField, Tooltip("Size of a door (depth into the diner, height) on the diner's right face.")]
        private Vector2 _doorSize = new Vector2(0.7f, 1.8f);
        [SerializeField, Min(0f), Tooltip("How far above the curb's top edge the side door (ambient people) sits, door centre.")]
        private float _sideDoorRise = 3f;
        [SerializeField, Min(0f), Tooltip("How far past the door line walkers go before they vanish into the diner.")]
        private float _doorDepth = 0.9f;

        [Header("Right strip")]
        [SerializeField, Min(1f), Tooltip("Width of the outside road and bus stop strip along the right wall, where ambient people appear.")]
        private float _stripWidth = 3.5f;
        [SerializeField, Min(0f), Tooltip("Open asphalt between the last bay column and the strip, room to swing out of the end bays.")]
        private float _stripGap = 3.5f;

        [Header("Walkways")]
        [SerializeField, Range(0.1f, 0.9f), Tooltip("Where the north-south crosswalk sits in the corridor between curb and bays (0 = curb, 1 = bays).")]
        private float _crossingShare = 0.4f;
        [SerializeField, Min(0.5f), Tooltip("Width of a walkway band (the crosswalks and the aisle crossing).")]
        private float _walkwayWidth = 1.4f;

        [Header("Roads below and above the lot")]
        [SerializeField, Min(0.5f), Tooltip("Width of one road lane.")]
        private float _laneWidth = 3.2f;
        [SerializeField, Min(5f), Tooltip("How far the road and its lane routes reach past the lot walls, to either side, so cars start off screen.")]
        private float _roadReach = 30f;
        [SerializeField, Min(0.5f), Tooltip("Radius of the turn from the entry road into the queue, and from the curb exit up in the top road.")]
        private float _turnRadius = 2.4f;
        [SerializeField, Min(0f), Tooltip("Opening kept beside the queue column in the south wall and beside the curb car in the top wall.")]
        private float _entryPadding = 0.3f;
        [SerializeField, Min(0f), Tooltip("Extra lot height above the side door, so a delivered car has room to straighten before the top wall gap (the lot grows taller by this much).")]
        private float _exitRunway = 1.5f;

        public float CurbGap => _curbGap;
        public float DinerWidth => _dinerWidth;
        public float DinerInset => _dinerInset;
        public Vector2 DoorSize => _doorSize;
        public float SideDoorRise => _sideDoorRise;
        public float DoorDepth => _doorDepth;
        public float StripWidth => _stripWidth;
        public float StripGap => _stripGap;
        public float CrossingShare => _crossingShare;
        public float WalkwayWidth => _walkwayWidth;
        public float LaneWidth => _laneWidth;
        public float RoadReach => _roadReach;
        public float TurnRadius => _turnRadius;
        public float EntryPadding => _entryPadding;
        public float ExitRunway => _exitRunway;
    }
}
