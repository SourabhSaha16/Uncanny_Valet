using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>
    /// The root of a hand-made lot: reads the markers under it, checks them with <see cref="HandmadeLotRules"/> and, when there is no error,
    /// builds the curb, bays and stations and answers as the lot's <see cref="ILotLayout"/> and <see cref="ISpotGeometry"/>. With an error it logs every
    /// finding naming this object, builds nothing and switches itself off. The root must not be rotated or scaled.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HandmadeLot : MonoBehaviour, ILotLayout, ISpotGeometry
    {
        [SerializeField, Tooltip("Shared numbers: settle time, speeds, curb facing, Perfect thresholds. Its geometry fields are ignored.")]
        private LotConfig _config;
        [SerializeField] private LotRequirements _requirements = new LotRequirements();
        [SerializeField, Tooltip("Use the camera rectangle below instead of the box round the gameplay markers.")]
        private bool _hasView;
        [SerializeField, Tooltip("World rectangle the camera must show.")] private Rect _view;

        private static readonly IReadOnlyList<Vector2> NoSlots = Array.Empty<Vector2>();

        private HandmadeLotBuilder _built;
        private AuthoredRoadLayout _road;
        private Vector2 _valetStart;
        private float _arrivalRotation;

        /// <summary>True once the lot passed every rule and was built.</summary>
        public bool IsBuilt => _built != null;
        public LotRequirements Requirements => _requirements;
        public bool HasAuthoredView => _hasView;
        public Rect AuthoredView => _view;
        /// <summary>The findings of the last check (errors and warnings).</summary>
        public IReadOnlyList<LotIssue> Issues { get; private set; } = Array.Empty<LotIssue>();
        /// <summary>The roads (lanes and routes); null until the lot is built.</summary>
        public IRoadLayout Road => _road;
        /// <summary>The walking routes; null until the lot is built.</summary>
        public IPedestrianPaths Paths { get; private set; }
        /// <summary>Perfect Park and drop-off thresholds, for Bootstrap to seed into the tuning store.</summary>
        public PerfectSettings Perfect => _config != null ? _config.Perfect : null;
        /// <summary>The world rectangle the camera must show: the authored view, or the box round the gameplay markers.</summary>
        public Rect ViewRect { get; private set; }

        public Vector2 ArrivalPoint => ArrivalSlots.Count > 0 ? ArrivalSlots[0] : Vector2.zero;
        public float ArrivalRotationDegrees => _arrivalRotation;
        public IReadOnlyList<Vector2> ArrivalSlots => _road != null ? _road.ArrivalSlots : NoSlots;
        public Vector2 ValetStartPoint => _valetStart;
        public IReadOnlyList<int> SpotIds => _built != null ? _built.Expander.OpenIds : Array.Empty<int>();
        public int RegularSpotCount => _built != null ? _built.RegularCount : 0;
        public int ChargingSpotCount => _built != null ? _built.ChargingCount : 0;
        public bool HasWashStation => WashStationCount > 0;
        public int WashStationCount => _built != null ? _built.WashCount : 0;

        public bool IsSpotFree(int spotId) => _built != null && _built.Occupancy.IsFree(spotId);

        public bool IsChargingSpot(int spotId) => _built != null && _built.Occupancy.IsCharging(spotId);

        public bool TryGetSpot(int spotId, out SpotBounds bay)
        {
            if (_built != null) return _built.Geometry.TryGetSpot(spotId, out bay);
            bay = default;
            return false;
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("The hand-made lot " + name + " has no LotConfig; assign one on its HandmadeLot component.", this);
                enabled = false;
                return;
            }
            var data = HandmadeLotReader.Read(this);
            var issues = new List<LotIssue>();
            HandmadeLotRules.Validate(data, issues);
            Issues = issues;
            if (Report(issues))
            {
                enabled = false;
                return;
            }
            _road = new AuthoredRoadLayout(data);
            Paths = new AuthoredPedestrianPaths(data);
            _valetStart = data.ValetStarts[0].Position;
            _arrivalRotation = data.Slots[0].Rect.RotationDegrees;
            ViewRect = LotBounds.View(data);
            var builder = new HandmadeLotBuilder(transform, _config, data);
            builder.Build();
            _built = builder;
        }

        // Logs every finding; true when at least one is an error.
        private bool Report(List<LotIssue> issues)
        {
            var failed = HandmadeLotRules.HasErrors(issues);
            foreach (var issue in issues)
            {
                if (issue.IsError) Debug.LogError("Hand-made lot " + name + " is not built: " + issue, this);
                else Debug.LogWarning("Hand-made lot " + name + ": " + issue, this);
            }
            return failed;
        }
    }
}
