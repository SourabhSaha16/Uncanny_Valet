using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Pure rule for the curb hand-back: the car must point down the screen (towards the road), within a tolerance.
    /// Car rotation is in degrees with 0 = nose up, so "down" is 180. Handles any angle, including wrap-around.
    /// </summary>
    public sealed class CurbFacingRule : ICurbFacing
    {
        /// <summary>Rotation of a car whose nose points straight down the screen.</summary>
        public const float DownDegrees = 180f;

        /// <summary>Unit world direction a car's nose points in when it faces the required way (0 = up, positive turns anticlockwise).</summary>
        public static Vector2 RequiredDirection
        {
            get
            {
                var radians = DownDegrees * Mathf.Deg2Rad;
                return new Vector2(-Mathf.Sin(radians), Mathf.Cos(radians));
            }
        }

        private const float FullTurn = 360f;
        private const float HalfTurn = 180f;

        private readonly float _toleranceDegrees;

        public CurbFacingRule(float toleranceDegrees)
        {
            _toleranceDegrees = toleranceDegrees < 0f ? 0f : toleranceDegrees;
        }

        /// <summary>Smallest absolute angle between a rotation and straight down, 0..180.</summary>
        public static float DeviationFromDown(float rotationDegrees)
        {
            var delta = (rotationDegrees - DownDegrees) % FullTurn;
            if (delta > HalfTurn) delta -= FullTurn;
            else if (delta < -HalfTurn) delta += FullTurn;
            return delta < 0f ? -delta : delta;
        }

        public bool IsAcceptable(float rotationDegrees) => DeviationFromDown(rotationDegrees) <= _toleranceDegrees;
    }
}
