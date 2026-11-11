using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>
    /// The anchor tables of the nine difficulty dials: which underlying director numbers each dial moves and by how
    /// much at the easy end, the default and the hard end. Read-only at runtime. A new asset starts with the shipped
    /// anchors (<see cref="DialAnchors"/>); edit the list to retune a dial without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Difficulty Dials Config", fileName = "DifficultyDialsConfig")]
    public sealed class DifficultyDialsConfig : ScriptableObject
    {
        [SerializeField, Min(0f), Tooltip("Dial value at the left end of the slider.")]
        private float _minPosition;
        [SerializeField, Min(0.5f), Tooltip("Where every dial starts. All underlying numbers keep their authored values here.")]
        private float _defaultPosition = 5f;
        [SerializeField, Min(1f), Tooltip("Dial value at the right end of the slider.")]
        private float _maxPosition = 10f;
        [SerializeField, Tooltip("Every dial-to-key link. See docs/redesign/difficulty_dials.md.")]
        private List<DialLink> _links = DialAnchors.All();

        [SerializeField, Tooltip("Where each dial starts in a fresh game and what its reset returns to (the designer baseline). A dial not listed starts at the default position.")]
        private List<DialStart> _startPositions = DialStart.Baseline();

        /// <summary>The dial's starting position: its baseline entry, or the default position when none is listed.</summary>
        public float StartOf(UncannyValet.Core.Events.TuningKey dial)
        {
            for (var i = 0; i < _startPositions.Count; i++)
                if (_startPositions[i].Dial == dial) return Mathf.Clamp(_startPositions[i].Position, _minPosition, _maxPosition);
            return _defaultPosition;
        }

        public float MinPosition => _minPosition;
        public float DefaultPosition => _defaultPosition;
        public float MaxPosition => _maxPosition;
        public IReadOnlyList<DialLink> Links => _links;

        private void OnValidate()
        {
            _defaultPosition = Mathf.Clamp(_defaultPosition, _minPosition + 0.01f, _maxPosition - 0.01f);
        }
    }
}
