using System;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>What the run needs from a hand-made lot (taken from the lot definition) and the tolerances the validator uses. Set on the lot root.</summary>
    [Serializable]
    public sealed class LotRequirements
    {
        [SerializeField, Min(1), Tooltip("Fewest queue slots the lot must have.")] private int _minQueueSlots = 1;
        [SerializeField, Min(1), Tooltip("Most queue slots the lot may have.")] private int _maxQueueSlots = 8;
        [SerializeField, Tooltip("The lot must have an EV charging bay (its modifier chances can ask for one).")] private bool _needsCharging;
        [SerializeField, Tooltip("The lot must have a wash bay.")] private bool _needsWash;
        [SerializeField, Tooltip("Ambient cars drive the back lanes, so the lot needs them.")] private bool _ambientTraffic;
        [SerializeField, Min(0), Tooltip("Ambient pedestrians per run; above 0 the lot needs spawn points.")] private int _ambientPedestrians;
        [SerializeField, Tooltip("Smallest a bay may be (width, length): a car must fit.")] private Vector2 _minBaySize = new Vector2(1.4f, 2.6f);
        [SerializeField, Min(0f), Tooltip("Largest gap between the curb and queue slot 0 that still counts as touching.")] private float _maxCurbGap = 1.5f;
        [SerializeField, Min(0f), Tooltip("How far a point may be from the lane it should lie on.")] private float _laneTolerance = 0.3f;
        [SerializeField, Min(0f), Tooltip("Radius of the corners the entry and exit routes are rounded with; 0 keeps the authored corners sharp.")] private float _turnRadius = 2f;

        public int MinQueueSlots { get => _minQueueSlots; set => _minQueueSlots = value; }
        public int MaxQueueSlots { get => _maxQueueSlots; set => _maxQueueSlots = value; }
        public bool NeedsCharging { get => _needsCharging; set => _needsCharging = value; }
        public bool NeedsWash { get => _needsWash; set => _needsWash = value; }
        public bool AmbientTraffic { get => _ambientTraffic; set => _ambientTraffic = value; }
        public int AmbientPedestrians { get => _ambientPedestrians; set => _ambientPedestrians = value; }
        public Vector2 MinBaySize { get => _minBaySize; set => _minBaySize = value; }
        public float MaxCurbGap { get => _maxCurbGap; set => _maxCurbGap = value; }
        public float LaneTolerance { get => _laneTolerance; set => _laneTolerance = value; }
        public float TurnRadius { get => _turnRadius; set => _turnRadius = value; }
    }
}
