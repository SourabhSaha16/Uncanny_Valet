using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>The Thunder stride car boost: multiplies the car's top speed and acceleration while it sits in the drive modifier stack.</summary>
    public sealed class ThunderBoostModifier : IDriveModifier
    {
        /// <summary>Runs after fault modifiers that edit steering or throttle.</summary>
        public const int BoostOrder = 100;

        private readonly float _topSpeedScale;
        private readonly float _accelerationScale;

        public ThunderBoostModifier(float topSpeedScale, float accelerationScale)
        {
            _topSpeedScale = Mathf.Max(0f, topSpeedScale);
            _accelerationScale = Mathf.Max(0f, accelerationScale);
        }

        public int Order => BoostOrder;

        public DriveInput Modify(DriveInput input) => input.Scaled(_topSpeedScale, _accelerationScale);
    }
}
