using UncannyValet.Core.Art;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Applies a <see cref="LightingPreset"/> to a lot: one URP 2D global light and the capped point lights under a "Lighting" group, no
    /// shadows. A preset that fails its validation is logged and not applied; a lot without a preset keeps the default 2D lighting.
    /// </summary>
    public static class LotLighting
    {
        /// <summary>Builds the lights; returns the group, or null when there is no preset or it is invalid.</summary>
        public static Transform Apply(Transform lot, LightingPreset preset)
        {
            if (preset == null) return null;
            if (!preset.TryValidate(out var problem))
            {
                Debug.LogError("Lighting preset not applied: " + problem, lot);
                return null;
            }
            var group = new GameObject("Lighting").transform;
            group.SetParent(lot, false);
            var global = Add(group, "Global Light", Light2D.LightType.Global, preset.GlobalColour, preset.GlobalIntensity);
            global.shadowsEnabled = false;
            var flicker = group.gameObject.AddComponent<LightFlicker>();
            for (var i = 0; i < preset.Lights.Count; i++) Point(group, flicker, preset.Lights[i], i);
            return group;
        }

        private static void Point(Transform group, LightFlicker flicker, PointLightSpec spec, int index)
        {
            var light = Add(group, "Point Light " + index, Light2D.LightType.Point, spec.Colour, spec.Intensity);
            light.transform.localPosition = spec.Position;
            light.pointLightOuterRadius = spec.Radius;
            light.pointLightInnerRadius = 0f;
            light.shadowsEnabled = false;
            if (spec.Flicker > 0f && spec.FlickerSpeed > 0f) flicker.Add(light, spec.Flicker, spec.FlickerSpeed);
        }

        private static Light2D Add(Transform group, string name, Light2D.LightType type, Color colour, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(group, false);
            var light = go.AddComponent<Light2D>();
            light.lightType = type;
            light.color = colour;
            light.intensity = intensity;
            return light;
        }
    }
}
