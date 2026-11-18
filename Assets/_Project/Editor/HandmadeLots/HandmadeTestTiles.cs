using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>The painted ground of the test lot: asphalt over 12 x 9 cells, a road centre line, two sidewalk strips with a crosswalk, a roof, bay markings and a hedge column.</summary>
    internal static class HandmadeTestTiles
    {
        private const int GroundMinX = -5, GroundMaxX = 6, GroundMinY = -5, GroundMaxY = 3;
        private const int SpeckleEvery = 5;

        public static void Paint(Grid grid)
        {
            Ground(Layer(grid, "Ground"));
            var sidewalk = Layer(grid, "Sidewalk");
            Fill(sidewalk, "sidewalk_plain", -2, -2, -1, 1);
            Fill(sidewalk, "sidewalk_plain", 0, 1, 5, 1);
            var marks = Layer(grid, "Markings");
            Fill(marks, "marking_centre_h", GroundMinX, -4, GroundMaxX, -4);
            for (var x = 1; x <= 4; x++) Fill(marks, "marking_bay_empty", x, 0, x, 0);
            Fill(marks, "marking_crosswalk_v", 0, 1, 0, 1);
            Roof(Layer(grid, "Building"));
            Fill(Layer(grid, "Hedge"), "hedge_block", 6, -3, 6, 1);
        }

        private static Tilemap Layer(Grid grid, string name) => grid.transform.Find(name).GetComponent<Tilemap>();

        private static void Ground(Tilemap map)
        {
            for (var x = GroundMinX; x <= GroundMaxX; x++)
                for (var y = GroundMinY; y <= GroundMaxY; y++)
                    Set(map, (x + y) % SpeckleEvery == 0 ? "ground_asphalt_speckle" : "ground_asphalt", x, y);
        }

        private static void Roof(Tilemap map)
        {
            Fill(map, "roof_floor", -4, -2, -3, 1);
            Fill(map, "roof_edge_w", -4, -1, -4, 0);
            Fill(map, "roof_edge_e", -3, -1, -3, 0);
            Fill(map, "roof_edge_n", -4, 1, -3, 1);
            Fill(map, "roof_edge_s", -4, -2, -3, -2);
        }

        private static void Fill(Tilemap map, string tile, int x0, int y0, int x1, int y1)
        {
            for (var x = x0; x <= x1; x++)
                for (var y = y0; y <= y1; y++) Set(map, tile, x, y);
        }

        private static void Set(Tilemap map, string tile, int x, int y)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TileBase>(HandmadeTileSetup.TileFolder + "/" + tile + ".asset");
            if (asset == null) Debug.LogError("Tile asset missing: " + tile + " (run Uncanny Valet/Setup/Hand-made Lot Kit).");
            map.SetTile(new Vector3Int(x, y, 0), asset);
        }
    }
}
