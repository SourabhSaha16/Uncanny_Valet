using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>
    /// Re-skins a sandbox world with the Valhalla Midgard art. The lot and the decor still build everything (colliders, bays, curb, routes); once they have, this hides their coloured
    /// rectangles (except the curb arrows, which show the delivery state) and draws the Valhalla sprites over the same geometry. Sandbox only: the game's scenes do not have it.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(SandboxWorldSlot))]
    public sealed class ValhallaLotSkin : MonoBehaviour
    {
        private const string PropsName = "Props";
        private const string RootName = "Valhalla Art";

        [SerializeField] private ValhallaArt _art;
        [SerializeField] private LotConfig _config;
        [SerializeField] private LotArtConfig _lotArt;
        [SerializeField, Tooltip("Keep the lot's own props (the fair stalls and the Ferris wheel of the crazy lot) instead of hiding them.")]
        private bool _keepProps;

        private void Start()
        {
            var slot = GetComponent<SandboxWorldSlot>();
            if (_art == null || !_art.IsComplete || _config == null || _lotArt == null || slot.Definition == null)
            {
                Debug.LogError("ValhallaLotSkin on " + name + " is missing its art or configs; rebuild with Setup/Build Everything.", this);
                return;
            }
            HideOwnArt(slot.Lot.transform, slot.Decor != null ? slot.Decor.transform : null);
            var root = new GameObject(RootName).transform;
            root.SetParent(transform, false);
            var g = new LotGeometry(_config, slot.Definition.QueueSlots, slot.Definition.LayoutOptions);
            var d = new DecorLayout(g, _config.WallThickness, _lotArt.SidewalkDepth, _lotArt.OutsideReach, _lotArt.BusStopSize, _lotArt.BusStopFromBottom);
            ValhallaGround.Build(root, _art, g, d, _config);
            ValhallaBays.Build(root, _art, g);
            ValhallaDiner.Build(root, _art, g);
            if (!_keepProps) ValhallaScenery.Build(root, _art, g);
        }

        // Switches the renderers (sprites and text labels) off, not the objects: their colliders and zones keep working.
        private void HideOwnArt(Transform lot, Transform decor)
        {
            var props = lot.Find(PropsName);
            if (!_keepProps && props != null && props.TryGetComponent<PropAnimator>(out var blink)) blink.enabled = false;
            Hide(lot, props);
            if (decor != null) Hide(decor, props);
        }

        private void Hide(Transform part, Transform props)
        {
            foreach (var renderer in part.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.GetComponentInParent<CurbArrow>() != null) continue;
                if (_keepProps && props != null && renderer.transform.IsChildOf(props)) continue;
                renderer.enabled = false;
            }
        }
    }
}
