using System.Collections.Generic;
using UncannyValet.Core.Art;
using UncannyValet.EditorTools.Art;
using UncannyValet.Lot;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>
    /// Hangs the lights of the stormy evening preset over the Vanaheim layout (the preset was drawn for a diner-sized lot): a lantern over each
    /// feast banner and a cool glint on every trough. The preset replaces its lights and stays inside its 16 point light cap.
    /// </summary>
    internal static class VanaheimLanterns
    {
        private const float LanternRadius = 4.2f;
        private const float LanternIntensity = 0.85f;
        private const float TroughRadius = 3.6f;
        private const float TroughIntensity = 0.55f;
        private const float LanternAboveBanner = 1.6f;
        private static readonly Color Lantern = new Color(1f, 0.71f, 0.35f);
        private static readonly Color Water = new Color(0.62f, 0.82f, 1f);

        public static void Hang(LotGeometry g)
        {
            var preset = AssetDatabase.LoadAssetAtPath<LightingPreset>(VanaheimArt.StormyPath);
            if (preset == null) return;
            var lights = new List<PointLightSpec>();
            var x0 = g.CrossingX + VanaheimProps.StallFromCrossing + VanaheimProps.BannerOffset;
            for (var i = 0; i < VanaheimProps.BannerCount; i++)
                lights.Add(new PointLightSpec(new Vector2(x0 + i * VanaheimProps.StallSpacing, g.Curb.yMax + VanaheimProps.BannerY + LanternAboveBanner), LanternRadius, Lantern,
                    LanternIntensity, 0.35f, 2f + i * 0.3f));
            foreach (var trough in g.Troughs) lights.Add(new PointLightSpec(trough.center, TroughRadius, Water, TroughIntensity));
            Write(preset, lights);
        }

        private static void Write(LightingPreset preset, List<PointLightSpec> lights)
        {
            var so = new SerializedObject(preset);
            var list = so.FindProperty("_lights");
            list.arraySize = lights.Count;
            for (var i = 0; i < lights.Count; i++)
            {
                var entry = list.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("_position").vector2Value = lights[i].Position;
                entry.FindPropertyRelative("_radius").floatValue = lights[i].Radius;
                entry.FindPropertyRelative("_colour").colorValue = lights[i].Colour;
                entry.FindPropertyRelative("_intensity").floatValue = lights[i].Intensity;
                entry.FindPropertyRelative("_flicker").floatValue = lights[i].Flicker;
                entry.FindPropertyRelative("_flickerSpeed").floatValue = lights[i].FlickerSpeed;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(preset);
        }
    }
}
