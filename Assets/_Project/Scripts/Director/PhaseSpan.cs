using System;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Director
{
    /// <summary>A named phase of the shift that starts at a fraction of the shift length.</summary>
    [Serializable]
    public struct PhaseSpan
    {
        [SerializeField] private DirectorPhase _phase;
        [SerializeField, Range(0f, 1f)] private float _start01;

        public PhaseSpan(DirectorPhase phase, float start01) { _phase = phase; _start01 = start01; }

        public DirectorPhase Phase => _phase;
        public float Start01 => _start01;
    }
}
