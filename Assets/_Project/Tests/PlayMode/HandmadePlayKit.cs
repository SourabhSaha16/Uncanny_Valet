using System.Reflection;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>
    /// Builds a minimal valid hand-made lot from marker objects: two regular bays, a curb, two queue slots, the entry and exit roads, a wall
    /// with a main door, wait points, a walk graph and a valet start. Pass <c>withCurb: false</c> to leave the curb out (a rule error).
    /// </summary>
    public static class HandmadePlayKit
    {
        public static readonly Vector2 Bay1 = new Vector2(6f, 0f);
        public static readonly Vector2 CurbCentre = Vector2.zero;
        public static readonly Vector2 Slot1 = new Vector2(0f, -10.2f);

        public static GameObject Build(LotConfig config, bool withCurb)
        {
            var root = new GameObject("Hand-made lot");
            root.SetActive(false);
            Set(root.AddComponent<HandmadeLot>(), "_config", config);
            Set(Add<LotBay>(root, "Bay 1", Bay1), "_number", 1);
            Set(Add<LotBay>(root, "Bay 2", new Vector2(10f, 0f)), "_number", 2);
            if (withCurb) Add<CurbZoneMarker>(root, "Curb", CurbCentre);
            Set(Add<QueueSlot>(root, "Slot 0", new Vector2(0f, -5.55f)), "_index", 0);
            Set(Add<QueueSlot>(root, "Slot 1", Slot1), "_index", 1);
            Roads(root);
            Set(Add<LotDoor>(root, "Main door", new Vector2(-7f, 0f)), "_kind", DoorKind.Main);
            Add<PatronWaitPoint>(root, "Wait", new Vector2(-3f, 0.5f));
            Add<ValetStart>(root, "Valet", new Vector2(3f, -1f));
            Add<SolidBlock>(root, "Wall", new Vector2(-8f, 0f)).GetComponent<BoxCollider2D>().size = new Vector2(1f, 10f);
            Graph(root);
            root.SetActive(true);
            return root;
        }

        private static void Roads(GameObject root)
        {
            var entry = Add<CarEntryRoad>(root, "Entry", Vector2.zero);
            Set(entry, "_lane", new[] { new Vector2(-30f, -14f), new Vector2(30f, -14f) });
            Set(entry, "_queueTurn", new Vector2(-2.4f, -14f));
            Set(entry, "_bend", new[] { new Vector2(0f, -11.6f) });
            var exit = Add<CarExitRoad>(root, "Exit", Vector2.zero);
            Set(exit, "_lane", new[] { new Vector2(-30f, 12f), new Vector2(30f, 12f) });
            Set(exit, "_curbExit", new[] { Vector2.zero, new Vector2(0f, 10f), new Vector2(2f, 12f) });
        }

        private static void Graph(GameObject root)
        {
            var a = Add<WalkNode>(root, "Node A", new Vector2(-6f, 0f));
            var b = Add<WalkNode>(root, "Node B", new Vector2(-3f, 0f));
            var c = Add<WalkNode>(root, "Node C", new Vector2(8f, -3f));
            foreach (var pair in new[] { (a, b), (b, c) })
            {
                var link = Add<WalkLink>(root, "Link", Vector2.zero);
                Set(link, "_a", pair.Item1);
                Set(link, "_b", pair.Item2);
            }
        }

        private static T Add<T>(GameObject parent, string name, Vector2 at) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = at;
            return go.AddComponent<T>();
        }

        private static void Set(object target, string field, object value)
            => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
