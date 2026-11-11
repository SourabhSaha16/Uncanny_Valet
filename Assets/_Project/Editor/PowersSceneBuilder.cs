using UncannyValet.Bootstrap;
using UncannyValet.Powers;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>Builds the Powers object of the Bootstrap scene: the meter, the activation and the Perfect feedback link.</summary>
    public static class PowersSceneBuilder
    {
        /// <summary>Creates the inactive "Powers" child; GameBootstrap activates it once the event bus is registered.</summary>
        public static PerfectFeedback Build(GameObject root, CameraFollow camera)
        {
            var go = new GameObject("Powers");
            go.transform.SetParent(root.transform, false);
            go.SetActive(false);
            var meter = go.AddComponent<PowerMeterController>();
            var activation = go.AddComponent<PowerActivationController>();
            var feedback = go.AddComponent<PerfectFeedback>();
            Set(meter, "_config", AssetDatabase.LoadAssetAtPath<PowerMeterConfig>(ProjectSetup.PowerMeterConfigPath));
            Set(activation, "_meter", meter);
            Set(feedback, "_camera", camera);
            return feedback;
        }

        private static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
