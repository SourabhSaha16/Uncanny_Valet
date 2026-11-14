using UncannyValet.Core.Art;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Draws the diner (one tall building on the left) from slices of a MetroCity building sprite: a tiled red roof, a
    /// visible front face of windows along the bottom, a striped awning over each door, the two doors and a small DINER
    /// sign; and the bus stop on the right strip. Purely visual: the diner's solid collider belongs to the lot.
    /// </summary>
    public static class LotDinerArt
    {
        private const int RoofOrder = 1;
        private const int FacadeOrder = 2;
        private const int AwningOrder = 3;
        private const int DoorOrder = 4;
        private const int SignOrder = 5;
        private const int SpriteOrder = 4;
        private const string DinerText = "DINER";
        private const string BusText = "BUS";
        private const float DinerLabelSize = 0.7f;
        private const float BusLabelSize = 0.5f;

        /// <param name="palette">When set, the building is re-skinned with its Wall and Roof colours (see <see cref="DinerSkin"/>).</param>
        public static void BuildDiner(Transform parent, RectFactory rects, LotGeometry g, LotArtConfig art, LotConfig config, WorldPalette palette = null)
        {
            var d = g.Diner;
            if (art.DinerBuilding == null)
            {
                Debug.LogError("LotArtConfig has no diner building sprite; rebuild with Setup/Build Everything.", art);
                return;
            }
            var texture = art.DinerBuilding.texture;
            var ppu = DinerSlices.PixelsPerUnit(art.DinerFacadePixels.width, d.width);
            var facadeHeight = DinerSlices.FacadeHeight(art.DinerFacadeBands, art.DinerFacadePixels.height, ppu, d.height);
            Tiled(parent, "Diner Roof", Slice(texture, art.DinerRoofPixels, ppu, palette),
                Rect.MinMaxRect(d.xMin, d.yMin + facadeHeight, d.xMax, d.yMax), RoofOrder);
            Tiled(parent, "Diner Facade", Slice(texture, art.DinerFacadePixels, ppu, palette),
                Rect.MinMaxRect(d.xMin, d.yMin, d.xMax, d.yMin + facadeHeight), FacadeOrder);
            Door(parent, rects, "Side Door", g.SideDoor, d.xMax, art);
            Door(parent, rects, "Main Door", g.MainDoor, d.xMax, art);
            var sign = new Vector2(d.center.x, d.yMin + facadeHeight + art.SignSize.y);
            rects.Make(parent, "Diner Sign", sign, art.SignSize, art.SignColor, SignOrder);
            SpotLabel.Create(parent, DinerText, sign, config, SignOrder + 1, DinerLabelSize);
        }

        public static void BuildBusStop(Transform parent, RectFactory rects, DecorLayout layout, LotArtConfig art, LotConfig config)
        {
            var stop = layout.BusStop;
            rects.Make(parent, "Bus Stop", stop.center, stop.size, art.BusStopColor, DoorOrder);
            SpotLabel.Create(parent, BusText, stop.center, config, SpriteOrder, BusLabelSize);
        }

        // A sprite cut from the building texture; it keeps the texture's point filtering and tiles without a mesh outline.
        private static Sprite Slice(Texture2D texture, RectInt pixels, float ppu, WorldPalette palette)
        {
            var rect = DinerSlices.TextureRect(pixels, texture.height);
            var skinned = palette != null ? DinerSkin.Slice(texture, rect, ppu, palette) : null;
            return skinned != null ? skinned : Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
        }

        private static void Tiled(Transform parent, string name, Sprite sprite, Rect area, int order)
        {
            if (area.height <= 0f) return;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = area.center;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = area.size;
            renderer.sortingOrder = order;
        }

        // The door on the diner's right face with a striped awning over it, flush with the face.
        private static void Door(Transform parent, RectFactory rects, string name, Rect door, float faceX, LotArtConfig art)
        {
            var awning = new Vector2(art.AwningSize.x, door.height + art.AwningSize.y);
            var stripes = Mathf.CeilToInt(awning.y / art.AwningStripe);
            var top = door.center.y + awning.y * 0.5f;
            for (var i = 0; i < stripes; i++)
            {
                var height = Mathf.Min(art.AwningStripe, awning.y - i * art.AwningStripe);
                var at = new Vector2(faceX - awning.x * 0.5f, top - i * art.AwningStripe - height * 0.5f);
                rects.Make(parent, name + " Awning " + i, at, new Vector2(awning.x, height),
                    i % 2 == 0 ? art.AwningColorA : art.AwningColorB, AwningOrder);
            }
            rects.Make(parent, name, door.center, door.size, art.DoorColor, DoorOrder);
        }
    }
}
