using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Reads the solids, hazard patches, solid scenery and tile layers under a lot root into the data (see <see cref="HandmadeLotReader"/>).</summary>
    internal static class HandmadeSceneryReader
    {
        public static void Read(HandmadeLot root, HandmadeLotData data)
        {
            foreach (var block in root.GetComponentsInChildren<SolidBlock>(true))
            {
                var box = block.GetComponent<BoxCollider2D>();
                var t = block.transform;
                var size = Vector2.Scale(box.size, new Vector2(Mathf.Abs(t.lossyScale.x), Mathf.Abs(t.lossyScale.y)));
                data.Solids.Add(new LotPiece(block.name, new OrientedRect(t.TransformPoint(box.offset), size, t.eulerAngles.z)));
            }
            foreach (var patch in root.GetComponentsInChildren<HazardPatch>(true))
                data.Hazards.Add(new LotPiece(patch.name, HandmadeLotReader.Rect(patch.transform, new Vector2(patch.Size, patch.Height))));
            foreach (var prop in root.GetComponentsInChildren<ScenerySolid>(true))
                if (prop.GetComponentInChildren<Collider2D>(true) == null) AddProp(prop, data);
            foreach (var layer in root.GetComponentsInChildren<LotTileLayer>(true))
                data.TileLayers.Add(new TileLayerData(layer.name, layer.Kind, layer.GetComponent<Collider2D>() != null));
        }

        // A prop without a collider is judged by the box of its sprite.
        private static void AddProp(ScenerySolid prop, HandmadeLotData data)
        {
            var sprite = prop.GetComponentInChildren<Renderer>(true);
            var bounds = sprite != null ? sprite.bounds : new Bounds(prop.transform.position, Vector3.one);
            data.SceneryWithoutCollider.Add(new LotPiece(prop.name, new OrientedRect(bounds.center, bounds.size, 0f)));
        }
    }
}
