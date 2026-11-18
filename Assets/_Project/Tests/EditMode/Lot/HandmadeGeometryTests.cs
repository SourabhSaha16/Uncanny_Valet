using System;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot.Handmade;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    /// <summary>OrientedRect, RouteRounder, RouteMath and WalkGraph.</summary>
    public class HandmadeGeometryTests
    {
        [Test]
        public void OrientedRect_QuarterTurn_SwapsTheAxes()
        {
            var r = new OrientedRect(Vector2.zero, new Vector2(2f, 6f), 90f);
            Assert.IsTrue(r.Contains(new Vector2(-2.9f, 0.4f)));
            Assert.IsFalse(r.Contains(new Vector2(0.4f, 2.9f)));
        }

        [Test]
        public void OrientedRect_Overlap_RotatedCrossOverlapsAndTouchingDoesNot()
        {
            var tall = new OrientedRect(Vector2.zero, new Vector2(1f, 6f), 0f);
            var turned = new OrientedRect(new Vector2(2f, 0f), new Vector2(1f, 6f), 90f);
            var beside = new OrientedRect(new Vector2(1f, 0f), new Vector2(1f, 6f), 0f);
            Assert.IsTrue(tall.Overlaps(turned));
            Assert.IsFalse(tall.Overlaps(beside));
        }

        [Test]
        public void OrientedRect_Segment_CrossingHitsAndGrazingDoesNot()
        {
            var r = new OrientedRect(Vector2.zero, new Vector2(2f, 2f), 45f);
            Assert.IsTrue(r.IntersectsSegment(new Vector2(-3f, 0f), new Vector2(3f, 0f)));
            Assert.IsFalse(r.IntersectsSegment(new Vector2(-3f, 2f), new Vector2(3f, 2f)));
            var box = new OrientedRect(Vector2.zero, new Vector2(2f, 2f), 0f);
            Assert.IsFalse(box.IntersectsSegment(new Vector2(-3f, 1f), new Vector2(3f, 1f)));
        }

        [Test]
        public void OrientedRect_Distance_IsTheGapBetweenEdges()
        {
            var a = new OrientedRect(Vector2.zero, new Vector2(2f, 2f), 0f);
            var b = new OrientedRect(new Vector2(5f, 0f), new Vector2(2f, 2f), 0f);
            Assert.AreEqual(3f, a.Distance(b), 1e-4f);
        }

        [Test]
        public void RouteMath_DistanceAlong_CountsFromTheStart()
        {
            var line = new[] { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(10f, 10f) };
            Assert.AreEqual(14f, RouteMath.DistanceAlong(new Vector2(10f, 4f), line), 1e-4f);
        }

        [Test]
        public void RouteRounder_RoundsACorner_KeepsEndsAndStaysInsideTheCorner()
        {
            var points = new[] { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(10f, 10f) };
            var rounded = new List<Vector2>();
            RouteRounder.Round(points, 3f, 6, rounded);
            Assert.AreEqual(points[0], rounded[0]);
            Assert.AreEqual(points[2], rounded[rounded.Count - 1]);
            Assert.Greater(rounded.Count, 3);
            foreach (var p in rounded) Assert.IsFalse(p.x > 10f + 1e-4f || p.y < -1e-4f, p.ToString());
            Assert.AreEqual(7f, rounded[1].x, 1e-4f);
        }

        [Test]
        public void RouteRounder_ZeroRadiusAndStraightLines_AreUnchanged()
        {
            var bent = new[] { new Vector2(0f, 0f), new Vector2(5f, 0f), new Vector2(5f, 5f) };
            var straight = new[] { new Vector2(0f, 0f), new Vector2(5f, 0f), new Vector2(9f, 0f) };
            var into = new List<Vector2>();
            RouteRounder.Round(bent, 0f, 6, into);
            CollectionAssert.AreEqual(bent, into);
            RouteRounder.Round(straight, 3f, 6, into);
            CollectionAssert.AreEqual(straight, into);
        }

        [Test]
        public void WalkGraph_FindsTheShortestWay()
        {
            var nodes = new List<NamedPoint>();
            foreach (var p in new[] { new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(5f, 1f), new Vector2(20f, 0f) })
                nodes.Add(new NamedPoint("n", p));
            var links = new List<WalkLinkData>
            {
                new WalkLinkData("a", 0, 1), new WalkLinkData("b", 0, 2), new WalkLinkData("c", 2, 1), new WalkLinkData("d", 1, 3),
            };
            var graph = new WalkGraph(nodes, links);
            Assert.IsTrue(graph.FindPath(0, 3));
            Assert.AreEqual(3, graph.PathCount);
            Assert.AreEqual(0, graph.PathNode(0));
            Assert.AreEqual(1, graph.PathNode(1));
            Assert.AreEqual(3, graph.PathNode(2));
        }

        [Test]
        public void WalkGraph_SeparateIslands_AreNotConnected()
        {
            var nodes = new List<NamedPoint> { new NamedPoint("a", Vector2.zero), new NamedPoint("b", Vector2.right), new NamedPoint("c", Vector2.up * 9f) };
            var graph = new WalkGraph(nodes, new List<WalkLinkData> { new WalkLinkData("l", 0, 1) });
            Assert.IsTrue(graph.Connected(0, 1));
            Assert.IsFalse(graph.Connected(0, 2));
            Assert.IsFalse(graph.FindPath(0, 2));
            Assert.AreEqual(2, graph.Nearest(new Vector2(0f, 8f)));
        }

        [Test]
        public void WalkGraph_FindPathAndNearest_AllocateNothing()
        {
            var data = HandmadeLotFixture.Valid();
            var graph = new WalkGraph(data.WalkNodes, data.WalkLinks);
            graph.FindPath(0, 3);
            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 50; i++)
            {
                graph.Nearest(new Vector2(1f, 1f));
                graph.FindPath(0, 3);
                graph.FindPath(3, 0);
            }
            Assert.AreEqual(before, GC.GetAllocatedBytesForCurrentThread());
        }
    }
}
