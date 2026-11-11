using System.Collections.Generic;

namespace UncannyValet.Lot
{
    /// <summary>Where Park Smash puts one candidate: an index into the candidate list and a bay id.</summary>
    public readonly struct SmashAssignment
    {
        public readonly int Candidate;
        public readonly int SpotId;
        public readonly bool Charged;
        public readonly bool Washed;
        public SmashAssignment(int candidate, int spotId, bool charged, bool washed)
        {
            Candidate = candidate; SpotId = spotId; Charged = charged; Washed = washed;
        }
    }

    /// <summary>
    /// Pure rule for Park Smash parking. Cars are served in the order given. An EV takes a free charging bay (and is charged
    /// to full); with none left it takes a regular bay. Every other car takes a regular bay, never a charging bay. A dirty car
    /// is washed. A car with no bay left is not assigned and stays where it is.
    /// </summary>
    public static class SmashPlan
    {
        private const float FacingUp = 0f;
        private const float FacingDown = 180f;

        /// <summary>Rotation in degrees of a car parked nose-in: bays above the aisle take the nose up, bays below take it down.</summary>
        public static float NoseInDegrees(float bayY, float aisleY) => bayY > aisleY ? FacingUp : FacingDown;

        /// <summary>Fills <paramref name="result"/> and returns how many cars were assigned.</summary>
        public static int Assign(IReadOnlyList<SmashCandidate> cars, IReadOnlyList<int> freeRegular,
            IReadOnlyList<int> freeCharging, List<SmashAssignment> result)
        {
            result.Clear();
            var regular = 0;
            var charging = 0;
            for (var i = 0; i < cars.Count; i++)
            {
                var car = cars[i];
                if (car.IsEv && charging < freeCharging.Count)
                    result.Add(new SmashAssignment(i, freeCharging[charging++], true, car.IsDirty));
                else if (regular < freeRegular.Count)
                    result.Add(new SmashAssignment(i, freeRegular[regular++], false, car.IsDirty));
            }
            return result.Count;
        }
    }
}
