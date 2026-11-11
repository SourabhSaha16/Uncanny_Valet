using UncannyValet.Cars.Fx;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// The live handling numbers every car reads, seeded from a <see cref="CarHandlingConfig"/> and changed by the F2 panel
    /// (the config asset itself stays read-only). One instance is shared by all cars of a factory.
    /// </summary>
    public sealed class CarHandlingLive
    {
        private readonly SkidConfig _skid;
        private float _lateralFriction, _forwardGrip, _gripLowSpeed, _gripHighSpeed, _gripAtSpeed, _driftGripScale;
        private float _driftMinSpeed, _turnFullSpeed, _rMin;
        private float _slideThreshold, _shiftSlideSpeed, _markOpacity, _markFadeSeconds;
        private float _topSpeedScale, _accelerationScale, _brakingScale, _coastDragScale, _reverseScale;

        public CarHandlingLive(CarHandlingConfig config)
        {
            _lateralFriction = config.LateralFriction;
            _forwardGrip = config.ForwardGrip;
            _gripLowSpeed = config.GripLowSpeed;
            _gripHighSpeed = config.GripHighSpeed;
            _gripAtSpeed = config.GripAtSpeed;
            _driftGripScale = config.DriftGripScale;
            _driftMinSpeed = config.DriftMinSpeed;
            _turnFullSpeed = config.TurnFullSpeed;
            _rMin = config.RMin;
            _skid = config.Skid;
            _slideThreshold = _skid.SlideThreshold;
            _shiftSlideSpeed = _skid.ShiftSlideSpeed;
            _markOpacity = _skid.MarkOpacity;
            _markFadeSeconds = _skid.MarkFadeSeconds;
            _topSpeedScale = config.TopSpeedScale;
            _accelerationScale = config.AccelerationScale;
            _brakingScale = config.BrakingScale;
            _coastDragScale = config.CoastDragScale;
            _reverseScale = config.ReverseScale;
        }

        /// <summary>One car's longitudinal numbers: its own values times the live scales.</summary>
        public DriveSettings DriveFor(CarDefinition car) => new DriveSettings(car.TopSpeed * _topSpeedScale,
            car.Acceleration * _accelerationScale, car.Braking * _brakingScale, car.CoastDrag * _coastDragScale,
            Mathf.Clamp(car.ReverseFactor * _reverseScale, 0.01f, 1f));

        /// <summary>The drift model's numbers.</summary>
        public DriftSettings Drift => new DriftSettings(_lateralFriction, _forwardGrip, _gripLowSpeed, _gripHighSpeed,
            _gripAtSpeed, _driftGripScale, _driftMinSpeed);

        /// <summary>The skid rule's numbers.</summary>
        public SkidSettings Skid => new SkidSettings(_slideThreshold, _shiftSlideSpeed, _markOpacity, _skid.ExitShare,
            _skid.FullDriftDegrees, _skid.FaintShare, _skid.HardBrakeSpeed);

        /// <summary>Seconds a skid mark takes to fade out.</summary>
        public float MarkFadeSeconds => _markFadeSeconds;
        /// <summary>Speed at which steering reaches full effect.</summary>
        public float TurnFullSpeed => _turnFullSpeed;
        /// <summary>Tightest radius at low speed.</summary>
        public float RMin => _rMin;
        /// <summary>Turn rate at full lock and full steering speed, radians per second.</summary>
        public float MaxTurnRate => TurningRadius.MaxTurnRate(_turnFullSpeed, _rMin);

        /// <summary>Writes the live values into the tuning store as its seeds.</summary>
        public void Seed(ITuning tuning)
        {
            tuning.Seed(TuningKey.HandlingLateralFriction, _lateralFriction);
            tuning.Seed(TuningKey.HandlingForwardGrip, _forwardGrip);
            tuning.Seed(TuningKey.HandlingGripLowSpeed, _gripLowSpeed);
            tuning.Seed(TuningKey.HandlingGripHighSpeed, _gripHighSpeed);
            tuning.Seed(TuningKey.HandlingGripAtSpeed, _gripAtSpeed);
            tuning.Seed(TuningKey.HandlingDriftGripScale, _driftGripScale);
            tuning.Seed(TuningKey.HandlingTurnFullSpeed, _turnFullSpeed);
            tuning.Seed(TuningKey.HandlingMinTurnRadius, _rMin);
            tuning.Seed(TuningKey.HandlingSlideThreshold, _slideThreshold);
            tuning.Seed(TuningKey.HandlingShiftSlideSpeed, _shiftSlideSpeed);
            tuning.Seed(TuningKey.HandlingMarkOpacity, _markOpacity);
            tuning.Seed(TuningKey.HandlingMarkFadeSeconds, _markFadeSeconds);
            tuning.Seed(TuningKey.HandlingTopSpeedScale, _topSpeedScale);
            tuning.Seed(TuningKey.HandlingAccelerationScale, _accelerationScale);
            tuning.Seed(TuningKey.HandlingBrakingScale, _brakingScale);
            tuning.Seed(TuningKey.HandlingCoastDragScale, _coastDragScale);
            tuning.Seed(TuningKey.HandlingReverseScale, _reverseScale);
        }

        /// <summary>Applies a tuning change; false when the key is not a handling key.</summary>
        public bool TrySet(TuningKey key, float value)
        {
            switch (key)
            {
                case TuningKey.HandlingLateralFriction: _lateralFriction = value; return true;
                case TuningKey.HandlingForwardGrip: _forwardGrip = value; return true;
                case TuningKey.HandlingGripLowSpeed: _gripLowSpeed = value; return true;
                case TuningKey.HandlingGripHighSpeed: _gripHighSpeed = value; return true;
                case TuningKey.HandlingGripAtSpeed: _gripAtSpeed = value; return true;
                case TuningKey.HandlingDriftGripScale: _driftGripScale = value; return true;
                case TuningKey.HandlingTurnFullSpeed: _turnFullSpeed = value; return true;
                case TuningKey.HandlingMinTurnRadius: _rMin = value; return true;
                case TuningKey.HandlingSlideThreshold: _slideThreshold = value; return true;
                case TuningKey.HandlingShiftSlideSpeed: _shiftSlideSpeed = value; return true;
                case TuningKey.HandlingMarkOpacity: _markOpacity = value; return true;
                case TuningKey.HandlingMarkFadeSeconds: _markFadeSeconds = value; return true;
                case TuningKey.HandlingTopSpeedScale: _topSpeedScale = value; return true;
                case TuningKey.HandlingAccelerationScale: _accelerationScale = value; return true;
                case TuningKey.HandlingBrakingScale: _brakingScale = value; return true;
                case TuningKey.HandlingCoastDragScale: _coastDragScale = value; return true;
                case TuningKey.HandlingReverseScale: _reverseScale = value; return true;
                default: return false;
            }
        }
    }
}
