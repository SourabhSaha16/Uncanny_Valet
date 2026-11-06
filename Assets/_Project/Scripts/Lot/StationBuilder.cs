using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Builds the wash slot as coloured rectangles; it starts inactive.</summary>
    public static class StationBuilder
    {
        private static readonly Color WashLine = new Color(0.45f, 0.95f, 0.92f);
        private static readonly Color WashFill = new Color(0.13f, 0.38f, 0.40f);
        private const string WashText = "WASH";

        public static StationZone BuildWash(RectFactory rects, Transform parent, LotConfig c, Rect area)
        {
            var root = new GameObject(StationKind.Wash + " Station");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = area.center;
            rects.Make(root.transform, "Line", Vector2.zero, area.size, WashLine, -7);
            rects.Make(root.transform, "Fill", Vector2.zero, area.size - Vector2.one * 0.3f, WashFill, -6);
            var size = LabelFit.Size(WashText.Length, area.width, c.StationLabelSize, c.LabelCharWidth, c.StationLabelFill);
            SpotLabel.Create(root.transform, WashText, Vector2.zero, c, -5, size);
            root.AddComponent<BoxCollider2D>();
            var zone = root.AddComponent<StationZone>();
            zone.Init(StationKind.Wash, c);
            root.SetActive(false);
            return zone;
        }
    }
}
