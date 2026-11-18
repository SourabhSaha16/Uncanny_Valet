using System.Collections.Generic;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.EditorTools.HandmadeLots
{
    /// <summary>
    /// Works out which walk nodes should be joined: every pair within <c>maxDistance</c> that is not linked yet, does not cross a solid and has
    /// no third node on the way (that node is linked instead, so the graph stays sparse). Pure: the window creates the links.
    /// </summary>
    public static class WalkAutoLinker
    {
        /// <summary>Radius of a node: another node this close to a candidate link counts as lying on it.</summary>
        public const float OnTheWay = 0.75f;

        /// <summary>The missing links as node index pairs (a below b).</summary>
        public static List<(int A, int B)> Plan(HandmadeLotData data, float maxDistance)
        {
            var plan = new List<(int, int)>();
            var nodes = data.WalkNodes;
            for (var a = 0; a < nodes.Count; a++)
                for (var b = a + 1; b < nodes.Count; b++)
                    if (Wanted(data, a, b, maxDistance)) plan.Add((a, b));
            return plan;
        }

        private static bool Wanted(HandmadeLotData data, int a, int b, float maxDistance)
        {
            var from = data.WalkNodes[a].Position;
            var to = data.WalkNodes[b].Position;
            if (Vector2.Distance(from, to) > maxDistance || Linked(data, a, b)) return false;
            for (var s = 0; s < data.Solids.Count; s++)
                if (data.Solids[s].Rect.IntersectsSegment(from, to)) return false;
            for (var n = 0; n < data.WalkNodes.Count; n++)
                if (n != a && n != b && RouteMath.DistanceToSegment(data.WalkNodes[n].Position, from, to) < OnTheWay) return false;
            return true;
        }

        private static bool Linked(HandmadeLotData data, int a, int b)
        {
            for (var i = 0; i < data.WalkLinks.Count; i++)
            {
                var link = data.WalkLinks[i];
                if ((link.A == a && link.B == b) || (link.A == b && link.B == a)) return true;
            }
            return false;
        }
    }
}
