using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The Midgard Diner of the Valhalla lot, seen from above: a roof with its sign and rooftop units, and a front facade of windows, doors and striped awnings along the sidewalk side.</summary>
    public static class ValhallaDiner
    {
        private const int RoofOrder = 1;
        private const int FrameOrder = 2;
        private const int FacadeOrder = 3;
        private const int AwningOrder = 4;
        private const float FacadeDepth = 2f;
        private const float AwningDepth = 0.9f;
        private const float WindowStep = 1.65f;
        private const float RoofTile = 0.8f;
        private const float TopBar = 0.45f;
        private const float BottomBar = 0.8f;
        private const float SideBar = 0.5f;
        private const float SignInset = 0.5f;
        private const float GadgetHeight = 1.5f;
        private const float GadgetInset = 1.6f;
        private const float LampHeight = 2f;
        private const float LampInset = 0.35f;
        private const float DoorTolerance = 0.9f;
        private const float PlanterHeight = 1.1f;
        private const float PlanterOffset = 1.3f;
        private const float QuarterTurn = -90f;

        /// <summary>Draws the diner and the things standing on its sidewalk under <paramref name="root"/>.</summary>
        public static void Build(Transform root, ValhallaArt art, LotGeometry g)
        {
            var d = g.Diner;
            var roof = Rect.MinMaxRect(d.xMin, d.yMin, d.xMax - FacadeDepth, d.yMax);
            Roof(root, art, roof);
            Facade(root, art, g, Rect.MinMaxRect(roof.xMax, d.yMin, d.xMax, d.yMax));
            Lamps(root, art, g);
        }

        private static void Roof(Transform root, ValhallaArt art, Rect roof)
        {
            ValhallaDraw.Tiled(root, "Roof", art.Get(ValhallaPiece.RoofFloor), roof, RoofOrder, RoofTile);
            ValhallaDraw.Fit(root, "Roof Top Bar", art.Get(ValhallaPiece.RoofFrameTop), new Rect(roof.xMin, roof.yMax - TopBar, roof.width, TopBar), FrameOrder);
            ValhallaDraw.Fit(root, "Roof Bottom Bar", art.Get(ValhallaPiece.RoofFrameBottom), new Rect(roof.xMin, roof.yMin, roof.width, BottomBar), FrameOrder);
            ValhallaDraw.Fit(root, "Roof Left Bar", art.Get(ValhallaPiece.RoofFrameLeft), new Rect(roof.xMin, roof.yMin, SideBar, roof.height), FrameOrder);
            var sign = art.Get(ValhallaPiece.DinerSign);
            var width = roof.width - SignInset * 2f;
            ValhallaDraw.Stand(root, "Diner Sign", sign, roof.center, width * sign.bounds.size.y / sign.bounds.size.x, FacadeOrder);
            var x = roof.xMin + GadgetInset;
            Gadget(root, art.Get(ValhallaPiece.GadgetHvacA), new Vector2(x, roof.yMax - GadgetInset));
            Gadget(root, art.Get(ValhallaPiece.GadgetHvacB), new Vector2(x, roof.yMin + GadgetInset));
            Gadget(root, art.Get(ValhallaPiece.GadgetDish), new Vector2(roof.xMax - GadgetInset, roof.yMax - GadgetInset));
            Gadget(root, art.Get(ValhallaPiece.GadgetDome), new Vector2(roof.xMax - GadgetInset, roof.yMin + GadgetInset));
        }

        private static void Gadget(Transform root, Sprite sprite, Vector2 at) => ValhallaDraw.Stand(root, sprite.name, sprite, at, GadgetHeight, FacadeOrder);

        // The front wall, a column of pieces turned a quarter so their cornice faces the sidewalk; the doors sit where patrons and ambient people enter.
        private static void Facade(Transform root, ValhallaArt art, LotGeometry g, Rect strip)
        {
            var count = Mathf.Max(1, Mathf.RoundToInt(strip.height / WindowStep));
            var step = strip.height / count;
            for (var i = 0; i < count; i++)
            {
                var cell = new Rect(strip.xMin, strip.yMax - (i + 1) * step, strip.width, step);
                var piece = PieceFor(cell.center.y, g, i);
                ValhallaDraw.Fit(root, "Facade " + i, art.Get(piece), cell, FacadeOrder, QuarterTurn);
                var awning = i % 2 == 0 ? ValhallaPiece.AwningMidA : ValhallaPiece.AwningMidB;
                var shade = new Rect(strip.xMax, cell.yMin + step * 0.05f, AwningDepth, step * 0.9f);
                ValhallaDraw.Fit(root, "Awning " + i, art.Get(awning), shade, AwningOrder, QuarterTurn);
            }
            var door = g.MainDoor.center;
            ValhallaDraw.Stand(root, "Door Planter", art.Get(ValhallaPiece.PlanterFlowers), new Vector2(strip.xMax + AwningDepth, door.y + PlanterOffset), PlanterHeight, AwningOrder);
        }

        private static ValhallaPiece PieceFor(float y, LotGeometry g, int index)
        {
            if (Mathf.Abs(y - g.MainDoor.center.y) < DoorTolerance) return ValhallaPiece.WallDoorDouble;
            if (Mathf.Abs(y - g.SideDoor.center.y) < DoorTolerance) return ValhallaPiece.WallDoorSingle;
            return ValhallaPiece.WallWindow1 + index % 4;
        }

        private static void Lamps(Transform root, ValhallaArt art, LotGeometry g)
        {
            var x = g.Curb.xMin - LampInset;
            var lamp = art.Get(ValhallaPiece.LampPost);
            ValhallaDraw.Stand(root, "Lamp 0", lamp, new Vector2(x, g.Diner.yMax - 1f), LampHeight, FacadeOrder);
            ValhallaDraw.Stand(root, "Lamp 1", lamp, new Vector2(x, g.Diner.center.y), LampHeight, FacadeOrder);
            ValhallaDraw.Stand(root, "Lamp 2", lamp, new Vector2(x, g.Diner.yMin + 1f), LampHeight, FacadeOrder);
        }
    }
}
