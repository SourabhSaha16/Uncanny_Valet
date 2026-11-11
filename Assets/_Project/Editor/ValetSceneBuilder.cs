using UncannyValet.Bootstrap;
using UncannyValet.EditorTools.Art;
using UncannyValet.Valet;
using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools
{
    /// <summary>Builds the valet object (body, layers, input, seat) and the follow camera for the Bootstrap scene.</summary>
    public static class ValetSceneBuilder
    {
        private const float ValetScale = 1.4f;
        private const float ValetColliderRadius = 0.3f; // times the scale: about the old 0.4 world radius
        private const int ValetSortingOrder = 20;
        private static readonly string[] ValetLayerNames = { "Model", "Outfit", "Hair" };

        public static GameObject BuildValet(GameObject parent, ValetArtConfig art)
        {
            var go = new GameObject("Valet");
            go.transform.SetParent(parent.transform, false);
            go.SetActive(false);
            go.transform.localScale = new Vector3(ValetScale, ValetScale, 1f);

            var body = go.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = ValetColliderRadius;
            var layers = AddValetLayers(go, art);

            go.AddComponent<KeyboardInputSource>();
            var mover = go.AddComponent<OnFootMover>();
            var interactor = go.AddComponent<Interactor>();
            var seat = go.AddComponent<SeatController>();
            var fx = new SerializedObject(go.AddComponent<ValetBoostFx>());
            fx.FindProperty("_shape").objectReferenceValue = layers[1]; // the outfit layer is the outlined shape
            fx.ApplyModifiedPropertiesWithoutUndo();

            var so = new SerializedObject(seat);
            so.FindProperty("_mover").objectReferenceValue = mover;
            so.FindProperty("_interactor").objectReferenceValue = interactor;
            SetArray(so.FindProperty("_bodyRenderers"), layers);
            SetArray(so.FindProperty("_bodyColliders"), collider);
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static SpriteRenderer[] AddValetLayers(GameObject valet, ValetArtConfig art)
        {
            var layers = new SpriteRenderer[ValetLayerNames.Length];
            for (var i = 0; i < layers.Length; i++)
            {
                var child = new GameObject(ValetLayerNames[i]);
                child.transform.SetParent(valet.transform, false);
                layers[i] = child.AddComponent<SpriteRenderer>();
                layers[i].sortingOrder = ValetSortingOrder + i;
            }
            var animator = valet.AddComponent<ValetAnimator>();
            var so = new SerializedObject(animator);
            so.FindProperty("_config").objectReferenceValue = art;
            so.FindProperty("_model").objectReferenceValue = layers[0];
            so.FindProperty("_outfit").objectReferenceValue = layers[1];
            so.FindProperty("_hair").objectReferenceValue = layers[2];
            so.ApplyModifiedPropertiesWithoutUndo();
            return layers;
        }

        public static CameraFollow BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(-9f, 0f, -10f); // valet start; CameraFollow snaps here
            var cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = CameraFollow.DefaultOrthographicSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.09f);
            return go.AddComponent<CameraFollow>();
        }

        private static void SetArray(SerializedProperty array, params Object[] items)
        {
            array.arraySize = items.Length;
            for (var i = 0; i < items.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
