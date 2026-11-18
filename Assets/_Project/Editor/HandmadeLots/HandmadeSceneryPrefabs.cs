using UnityEditor;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// Free-standing scenery prefabs (trees, bushes, bench, lamp post, bin, hydrant, mailbox, planters, cone, bollard, fence, signs, roof gadgets) from the
    /// sliced Valhalla props, for placing by hand on top of the tilemaps. Paint only: no collider (a lot adds its own solids).
    /// </summary>
    public static class HandmadeSceneryPrefabs
    {
        private const string LotSpriteFolder = "Assets/_Project/Art/ValhallaMidgard/Lot/";
        private const string PrefabPrefix = "Scenery_";
        /// <summary>Sorting order of scenery: above the ground layers and the Building tilemap, below the Overhead tilemap.</summary>
        private const int SceneryOrder = 3;
        /// <summary>The props are cut at the source's size; the sandbox draws them at about this share of it so a bench fits a bay.</summary>
        public const float PropScale = 0.7f;

        /// <summary>Sprite file names (without .png) of every scenery prop, and the prefab name each one gets.</summary>
        public static readonly (string Sprite, string Name)[] Props =
        {
            ("tree_large", "TreeLarge"), ("tree_small", "TreeSmall"), ("bush", "Bush"), ("planter_flowers", "PlanterFlowers"),
            ("hedge_planter", "HedgePlanter"), ("flowerbox_large", "FlowerboxLarge"), ("flowerbox_medium", "FlowerboxMedium"),
            ("flowerbox_small", "FlowerboxSmall"), ("bench", "Bench"), ("lamp_post", "LampPost"), ("bin", "Bin"), ("hydrant", "Hydrant"),
            ("meter", "Meter"), ("fence", "Fence"), ("bollard", "Bollard"), ("cone", "Cone"), ("mailbox", "Mailbox"),
            ("newspaper_box", "NewspaperBox"), ("sign_coffee", "SignCoffee"), ("open_sign", "OpenSign"), ("boom_barrier", "BoomBarrier"),
            ("diner_sign", "DinerSign"), ("diner_sign_vertical", "DinerSignVertical"), ("gadget_dome", "GadgetDome"),
            ("gadget_hvac_a", "GadgetHvacA"), ("gadget_hvac_b", "GadgetHvacB"), ("gadget_panel", "GadgetPanel"), ("gadget_dish", "GadgetDish"),
        };

        /// <summary>The prefab path of a prop name.</summary>
        public static string PathOf(string name) => HandmadeTileSetup.SceneryFolder + "/" + PrefabPrefix + name + ".prefab";

        /// <summary>Creates or refreshes every scenery prefab.</summary>
        public static void Build()
        {
            foreach (var (sprite, name) in Props)
            {
                var art = AssetDatabase.LoadAssetAtPath<Sprite>(LotSpriteFolder + sprite + ".png");
                if (art == null)
                {
                    Debug.LogError("Scenery sprite missing: " + LotSpriteFolder + sprite + ".png (run tools/slice-valhalla.py).");
                    continue;
                }
                var go = new GameObject(PrefabPrefix + name);
                go.transform.localScale = new Vector3(PropScale, PropScale, 1f);
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = art;
                renderer.sortingOrder = SceneryOrder;
                PrefabUtility.SaveAsPrefabAsset(go, PathOf(name));
                Object.DestroyImmediate(go);
            }
        }
    }
}
