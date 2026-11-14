using System;
using UnityEngine;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Tunables of the valet pickup spot and the queue behind it (issue 127). Slot 0 of the lot's arrival queue is the pickup
    /// spot; the other slots are the queue. Lives inside <see cref="PatronConfig"/>.
    /// </summary>
    [Serializable]
    public sealed class PickupQueueSettings
    {
        [SerializeField, Range(1, 8), Tooltip("Cars that may wait at once, the pickup spot included (1 pickup spot + 3 queue slots = 4). The lot's queue slots limit it too. A full queue holds the next arrival.")]
        private int _maxQueuedCars = 4;
        [SerializeField, Min(0.5f), Tooltip("World units from the valet (on foot or driving) to the car in the pickup spot at which its patron gets out and walks to the diner.")]
        private float _approachRange = 4f;
        [SerializeField, Min(0.5f), Tooltip("Units per second a queued car drives forward when the car ahead has left.")]
        private float _advanceSpeed = 5f;
        [SerializeField, Tooltip("Width and length of the box that must be free of cars before the car behind drives into a slot; a little smaller than a car.")]
        private Vector2 _advanceClearSize = new Vector2(2f, 4f);
        [SerializeField, Min(0.2f), Tooltip("How far from a slot centre a standing car still counts as being in that slot.")]
        private float _slotTolerance = 1.5f;

        public int MaxQueuedCars => _maxQueuedCars;
        public float ApproachRange => _approachRange;
        public float AdvanceSpeed => _advanceSpeed;
        public Vector2 AdvanceClearSize => _advanceClearSize;
        public float SlotTolerance => _slotTolerance;
    }
}
