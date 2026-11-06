using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Builds the charging bays and the wash slot as coloured rectangles; each starts inactive.</summary>
    public static class StationBuilder
    {
        private static readonly Color ChargeLine = new Color(0.30f, 0.60f, 1f);
        private static readonly Color ChargeFill = new Color(0.14f, 0.22f, 0.40f);
        private static readonly Color WashLine = new Color(0.45f, 0.95f, 0.92f);
        private static readonly Color WashFill = new Color(0.13f, 0.38f, 0.40f);

        public static StationZone[] Build(RectFactory rects, Transform parent, LotConfig c, Rect[] areas)
        {
            var zones = new StationZone[areas.Length];
            for (var i = 0; i < areas.Length; i++)
            {
                var wash = i == areas.Length - 1;
                zones[i] = wash
                    ? Make(rects, parent, c, areas[i], StationKind.Wash, "WASH", WashLine, WashFill)
                    : Make(rects, parent, c, areas[i], StationKind.Charging, "CHARGE", ChargeLine, ChargeFill);
            }
            return zones;
        }

        private static StationZone Make(RectFactory rects, Transform parent, LotConfig c, Rect area,
            StationKind kind, string text, Color line, Color fill)
        {
            var root = new GameObject(kind + " Station");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = area.center;
            rects.Make(root.transform, "Line", Vector2.zero, area.size, line, -7);
            rects.Make(root.transform, "Fill", Vector2.zero, area.size - Vector2.one * 0.3f, fill, -6);
            var size = LabelFit.Size(text.Length, area.width, c.StationLabelSize, c.LabelCharWidth, c.StationLabelFill);
            SpotLabel.Create(root.transform, text, Vector2.zero, c, -5, size);
            root.AddComponent<BoxCollider2D>();
            var zone = root.AddComponent<StationZone>();
            zone.Init(kind, c);
            root.SetActive(false);
            return zone;
        }
    }
}
