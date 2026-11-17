using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>The Thunder stride car boost: multiplies the car's top speed and acceleration, and eases speed grip loss and the turn fall-off, while it sits in the drive modifier stack.</summary>
    public sealed class ThunderBoostModifier : IDriveModifier
    {
        /// <summary>Runs after fault modifiers that edit steering or throttle.</summary>
        public const int BoostOrder = 100;

        private readonly float _topSpeedScale;
        private readonly float _accelerationScale;
        private readonly float _gripLossScale;
        private readonly float _turnFalloffScale;

        public ThunderBoostModifier(float topSpeedScale, float accelerationScale, float gripLossScale = 1f,
            float turnFalloffScale = 1f)
        {
            _topSpeedScale = Mathf.Max(0f, topSpeedScale);
            _accelerationScale = Mathf.Max(0f, accelerationScale);
            _gripLossScale = Mathf.Max(0f, gripLossScale);
            _turnFalloffScale = Mathf.Max(0f, turnFalloffScale);
        }

        public int Order => BoostOrder;

        public DriveInput Modify(DriveInput input) => input.Scaled(_topSpeedScale, _accelerationScale)
            .WithGripLossScaled(_gripLossScale).WithTurnFalloffScaled(_turnFalloffScale);
    }
}
