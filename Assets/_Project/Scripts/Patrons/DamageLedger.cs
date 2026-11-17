using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Patrons
{
    /// <summary>
    /// Remembers each car's damage (0..1) from <see cref="CarDamaged"/> until its ticket is paid or voided, so the blessing can
    /// be reduced at payout. Forgets everything when a shift starts.
    /// </summary>
    public sealed class DamageLedger
    {
        private readonly IEventBus _bus;
        private readonly Dictionary<CarId, float> _damage = new Dictionary<CarId, float>();

        public DamageLedger(IEventBus bus)
        {
            _bus = bus;
            _bus.Subscribe<CarDamaged>(OnDamaged);
            _bus.Subscribe<ShiftStarted>(OnShiftStarted);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<CarDamaged>(OnDamaged);
            _bus.Unsubscribe<ShiftStarted>(OnShiftStarted);
        }

        /// <summary>The car's damage so far, 0 when it has none.</summary>
        public float Damage01(CarId car) => _damage.TryGetValue(car, out var d) ? d : 0f;

        /// <summary>Returns the car's damage and forgets it (its ticket was paid).</summary>
        public float Take(CarId car)
        {
            var d = Damage01(car);
            _damage.Remove(car);
            return d;
        }

        /// <summary>Forgets the car's damage (its ticket closed without a payout).</summary>
        public void Clear(CarId car) => _damage.Remove(car);

        private void OnDamaged(CarDamaged e) => _damage[e.Car] = e.Damage01;

        private void OnShiftStarted(ShiftStarted e) => _damage.Clear();
    }
}
