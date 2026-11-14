using System.Collections.Generic;
using UnityEngine;

namespace UncannyValet.Core.Art
{
    /// <summary>
    /// The lighting of a lot: the URP 2D global light colour and intensity plus a capped list of 2D point lights (string lights, lamps,
    /// the Ferris wheel glow). No shadows, to keep the Web build fast. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Art/Lighting Preset", fileName = "LightingPreset")]
    public sealed class LightingPreset : ScriptableObject
    {
        /// <summary>Most point lights a lot may have.</summary>
        public const int MaxLights = 16;

        [SerializeField, Tooltip("Colour of the global light that lights everything (day white, night blue).")]
        private Color _globalColour = Color.white;
        [SerializeField, Range(0f, 2f), Tooltip("Strength of the global light. 1 leaves sprites as drawn; lower is darker.")]
        private float _globalIntensity = 1f;
        [SerializeField, Tooltip("The point lights, at most 16.")]
        private List<PointLightSpec> _lights = new List<PointLightSpec>();

        public Color GlobalColour => _globalColour;
        public float GlobalIntensity => _globalIntensity;
        public IReadOnlyList<PointLightSpec> Lights => _lights;

        /// <summary>True when the preset is usable (at most 16 lights, positive radii, sane intensity); otherwise <paramref name="problem"/> names the fault.</summary>
        public bool TryValidate(out string problem)
        {
            problem = null;
            if (_globalIntensity < 0f) problem = name + ": the global intensity is negative";
            else if (_lights.Count > MaxLights) problem = name + ": " + _lights.Count + " lights; the cap is " + MaxLights;
            for (var i = 0; problem == null && i < _lights.Count; i++)
            {
                if (_lights[i].Radius <= 0f) problem = name + ": light " + i + " has no radius";
                else if (_lights[i].Intensity < 0f) problem = name + ": light " + i + " has a negative intensity";
            }
            return problem == null;
        }

        /// <summary>Makes a preset (used by the setup tool and tests).</summary>
        public static LightingPreset Create(string presetName, Color globalColour, float globalIntensity, PointLightSpec[] lights)
        {
            var preset = CreateInstance<LightingPreset>();
            preset.name = presetName;
            preset._globalColour = globalColour;
            preset._globalIntensity = globalIntensity;
            preset._lights = new List<PointLightSpec>(lights);
            return preset;
        }
    }
}
