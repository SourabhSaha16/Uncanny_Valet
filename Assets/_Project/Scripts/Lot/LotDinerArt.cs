using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>
    /// Draws the diner (one tall building on the left with a side door and a main door) and the bus stop on the right
    /// strip from plain rectangles (placeholder art until a tall MetroCity building is authored). Purely visual: the
    /// diner's solid collider belongs to the lot.
    /// </summary>
    public static class LotDinerArt
    {
        private const int WallOrder = 1;
        private const int RoofOrder = 2;
        private const int DoorOrder = 3;
        private const int SpriteOrder = 4;
        private const string DinerText = "DINER";
        private const string BusText = "BUS";
        private const float DinerLabelSize = 1.4f;
        private const float BusLabelSize = 0.5f;

        public static void BuildDiner(Transform parent, RectFactory rects, LotGeometry g, LotArtConfig art, LotConfig config)
        {
            var d = g.Diner;
            rects.Make(parent, "Diner Wall", d.center, d.size, art.DinerWallColor, WallOrder);
            var roofCentre = d.center;
            rects.Make(parent, "Diner Roof", roofCentre, d.size - Vector2.one * art.DinerRoofInset * 2f, art.DinerRoofColor, RoofOrder);
            SpotLabel.Create(parent, DinerText, roofCentre, config, SpriteOrder + 1, DinerLabelSize);
            rects.Make(parent, "Side Door", g.SideDoor.center, g.SideDoor.size, art.DoorColor, DoorOrder);
            rects.Make(parent, "Main Door", g.MainDoor.center, g.MainDoor.size, art.DoorColor, DoorOrder);
        }

        public static void BuildBusStop(Transform parent, RectFactory rects, DecorLayout layout, LotArtConfig art, LotConfig config)
        {
            var stop = layout.BusStop;
            rects.Make(parent, "Bus Stop", stop.center, stop.size, art.BusStopColor, DoorOrder);
            SpotLabel.Create(parent, BusText, stop.center, config, SpriteOrder, BusLabelSize);
        }
    }
}
