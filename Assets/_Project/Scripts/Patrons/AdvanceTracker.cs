using System.Collections.Generic;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Remembers which queued cars are driving up and into which slot. A drive that never reports back (the car was
    /// interrupted) expires after its deadline, so the queue offers the move again instead of freezing.
    /// </summary>
    public sealed class AdvanceTracker
    {
        private readonly Dictionary<CarId, int> _targets = new Dictionary<CarId, int>();
        private readonly Dictionary<CarId, float> _deadlines = new Dictionary<CarId, float>();
        private readonly List<CarId> _expired = new List<CarId>();

        /// <summary>True while the car is driving up.</summary>
        public bool IsMoving(CarId car) => _targets.ContainsKey(car);

        /// <summary>Records a drive into <paramref name="slot"/> that counts as interrupted after <paramref name="deadline"/>.</summary>
        public void Start(CarId car, int slot, float deadline)
        {
            _targets[car] = slot;
            _deadlines[car] = deadline;
        }

        public void Remove(CarId car)
        {
            _targets.Remove(car);
            _deadlines.Remove(car);
        }

        public void Clear()
        {
            _targets.Clear();
            _deadlines.Clear();
        }

        /// <summary>Marks every slot a car is driving into as occupied.</summary>
        public void MarkTargets(List<bool> occupied)
        {
            foreach (var target in _targets.Values) if (target < occupied.Count) occupied[target] = true;
        }

        /// <summary>Forgets drives whose deadline has passed at <paramref name="now"/>.</summary>
        public void Expire(float now)
        {
            _expired.Clear();
            foreach (var pair in _deadlines) if (now > pair.Value) _expired.Add(pair.Key);
            for (var i = 0; i < _expired.Count; i++) Remove(_expired[i]);
        }
    }
}
