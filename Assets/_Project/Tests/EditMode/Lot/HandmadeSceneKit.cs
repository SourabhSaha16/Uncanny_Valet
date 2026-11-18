using System.Reflection;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>Builds the same small lot as <see cref="HandmadeLotFixture"/> out of in-memory marker GameObjects, with the lot root at <c>offset</c>.</summary>
    public static class HandmadeSceneKit
    {
        public static HandmadeLot Build(Vector2 offset, LotConfig config)
        {
            var root = new GameObject("Lot");
            root.SetActive(false);
            root.transform.position = offset;
            var lot = root.AddComponent<HandmadeLot>();
            Set(lot, "_config", config);
            Bay(root, "Bay 1", BayKind.Regular, 1, new Vector2(6f, 0f));
            Bay(root, "Bay 2", BayKind.Regular, 2, new Vector2(10f, 0f));
            Curb(root);
            Slot(root, 0, new Vector2(0f, -5.55f));
            Slot(root, 1, new Vector2(0f, -10.2f));
            Roads(root);
            People(root);
            var wall = Add<SolidBlock>(root, "Wall", new Vector2(-8f, 0f));
            wall.GetComponent<BoxCollider2D>().size = new Vector2(1f, 10f);
            Add<ValetStart>(root, "Valet", new Vector2(3f, -1f));
            return lot;
        }

        public static T Add<T>(GameObject parent, string name, Vector2 localPosition) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPosition;
            return go.AddComponent<T>();
        }

        public static void Set(object target, string field, object value)
            => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        private static void Bay(GameObject root, string name, BayKind kind, int number, Vector2 at)
        {
            var bay = Add<LotBay>(root, name, at);
            Set(bay, "_kind", kind);
            Set(bay, "_number", number);
        }

        private static void Curb(GameObject root) => Add<CurbZoneMarker>(root, "Curb", Vector2.zero);

        private static void Slot(GameObject root, int index, Vector2 at) => Set(Add<QueueSlot>(root, "Slot " + index, at), "_index", index);

        private static void Roads(GameObject root)
        {
            var entry = Add<CarEntryRoad>(root, "Entry", Vector2.zero);
            Set(entry, "_lane", new[] { new Vector2(-30f, -14f), new Vector2(30f, -14f) });
            Set(entry, "_queueTurn", new Vector2(-2.4f, -14f));
            Set(entry, "_bend", new[] { new Vector2(0f, -11.6f) });
            var exit = Add<CarExitRoad>(root, "Exit", Vector2.zero);
            Set(exit, "_lane", new[] { new Vector2(-30f, 12f), new Vector2(30f, 12f) });
            Set(exit, "_curbExit", new[] { Vector2.zero, new Vector2(0f, 10f), new Vector2(2f, 12f) });
            var back = Add<AmbientLane>(root, "Entry back", Vector2.zero);
            Set(back, "_points", new[] { new Vector2(30f, -17f), new Vector2(-30f, -17f) });
        }

        private static void People(GameObject root)
        {
            Set(Add<LotDoor>(root, "Main door", new Vector2(-7f, 0f)), "_kind", DoorKind.Main);
            Set(Add<LotDoor>(root, "Side door", new Vector2(-7f, 4f)), "_kind", DoorKind.Side);
            Set(Add<PatronWaitPoint>(root, "Wait B", new Vector2(-3f, -0.5f)), "_order", 2);
            Set(Add<PatronWaitPoint>(root, "Wait A", new Vector2(-3f, 0.5f)), "_order", 1);
            Add<AmbientSpawnPoint>(root, "Spawn", new Vector2(14f, 4f));
            var points = new[] { new Vector2(-6f, 0f), new Vector2(-3f, 0f), new Vector2(3f, 4f), new Vector2(12f, 4f), new Vector2(8f, -3f) };
            var nodes = new WalkNode[points.Length];
            for (var i = 0; i < points.Length; i++) nodes[i] = Add<WalkNode>(root, "Node " + i, points[i]);
            Link(root, nodes[0], nodes[1]);
            Link(root, nodes[1], nodes[2]);
            Link(root, nodes[2], nodes[3]);
            Link(root, nodes[3], nodes[4]);
            Link(root, nodes[4], nodes[1]);
        }

        private static void Link(GameObject root, WalkNode a, WalkNode b)
        {
            var link = Add<WalkLink>(root, "Link", Vector2.zero);
            Set(link, "_a", a);
            Set(link, "_b", b);
        }
    }
}
