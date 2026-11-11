using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Lot
{
    /// <summary>Remembers how each car in the pickup zone faces and works out the arrow state, asking the patron side who would accept.</summary>
    public sealed class CurbArrowTracker
    {
        private readonly Dictionary<CarId, bool> _facingByCar = new Dictionary<CarId, bool>();

        /// <summary>Records a car's facing; true when this changed what is known.</summary>
        public bool Set(CarId car, bool facingRight)
        {
            if (_facingByCar.TryGetValue(car, out var known) && known == facingRight) return false;
            _facingByCar[car] = facingRight;
            return true;
        }

        /// <summary>Forgets a car that left; true when it was known.</summary>
        public bool Remove(CarId car) => _facingByCar.Remove(car);

        /// <summary>The state now; <paramref name="accept"/> may be null (nobody accepts).</summary>
        public CurbArrowState Evaluate(IPickupAcceptance accept)
        {
            int accepted = 0, wrong = 0;
            foreach (var pair in _facingByCar)
            {
                if (!pair.Value) wrong++;
                else if (accept != null && accept.WillAccept(pair.Key)) accepted++;
            }
            return CurbArrowRule.Evaluate(accepted, wrong);
        }
    }
}
