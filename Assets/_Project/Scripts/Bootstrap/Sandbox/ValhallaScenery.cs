using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The street furniture of the Valhalla lot: trees along the right strip, a bench and a bin, a hydrant, a mailbox, a coffee sign and a hedge planter. None of it blocks anything; it is paint.</summary>
    public static class ValhallaScenery
    {
        private const int Order = 3;
        private const float TreeHeight = 3.2f;
        private const float TreeInset = 2.2f;
        private const float TreeSpacing = 3.4f;
        private const float BenchHeight = 1.6f;
        private const float BenchFromBottom = 3f;
        private const float BinHeight = 1f;
        private const float BinFromBottom = 1.2f;
        private const float PostHeight = 1.4f;
        private const float SignHeight = 1.5f;
        private const float PlanterHeight = 1f;
        private const float PlanterInset = 0.6f;
        private const float SignOffset = 1.6f;
        private const float QuarterTurn = 90f;

        /// <summary>Draws the pieces under <paramref name="root"/>.</summary>
        public static void Build(Transform root, ValhallaArt art, LotGeometry g)
        {
            var tree = art.Get(ValhallaPiece.TreeLarge);
            var x = g.Strip.center.x;
            var ys = new[] { g.Strip.yMax - TreeInset, g.Strip.yMax - TreeInset - TreeSpacing, g.AisleY - TreeSpacing, g.AisleY - 2f * TreeSpacing };
            for (var i = 0; i < ys.Length; i++) ValhallaDraw.Stand(root, "Tree " + i, tree, new Vector2(x, ys[i]), TreeHeight, Order);
            ValhallaDraw.Stand(root, "Bench", art.Get(ValhallaPiece.Bench), new Vector2(x, g.Strip.yMin + BenchFromBottom), BenchHeight, Order, QuarterTurn);
            ValhallaDraw.Stand(root, "Bin", art.Get(ValhallaPiece.Bin), new Vector2(x, g.Strip.yMin + BinFromBottom), BinHeight, Order);
            ValhallaDraw.Stand(root, "Hydrant", art.Get(ValhallaPiece.Hydrant), new Vector2(x, g.AisleY + TreeSpacing), PostHeight, Order);
            ValhallaDraw.Stand(root, "Mailbox", art.Get(ValhallaPiece.Mailbox), new Vector2(x, g.Strip.yMax - TreeInset - 2f * TreeSpacing), PostHeight, Order);
            ValhallaDraw.Stand(root, "Coffee Sign", art.Get(ValhallaPiece.SignCoffee), g.MainDoor.center + new Vector2(SignOffset, -SignOffset), SignHeight, Order);
            ValhallaDraw.Stand(root, "Hedge Planter", art.Get(ValhallaPiece.HedgePlanter), new Vector2(g.Diner.center.x, g.Bounds.yMin + PlanterInset), PlanterHeight, Order);
        }
    }
}
