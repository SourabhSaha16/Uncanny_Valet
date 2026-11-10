using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Lot
{
    public class RouteMarksTests
    {
        [Test]
        public void MarksAreSpacedAlongAStraightRoute()
        {
            var route = new List<Vector2> { Vector2.zero, new Vector2(0f, -10f) };
            var marks = RouteMarks.Along(route, 1f, 3f);
            Assert.AreEqual(4, marks.Count);
            Assert.AreEqual(-1f, marks[0].Position.y, 1e-4f);
            Assert.AreEqual(-10f, marks[3].Position.y, 1e-4f);
            foreach (var mark in marks) Assert.AreEqual(Vector2.down, mark.Direction);
        }

        [Test]
        public void SpacingCarriesAroundACorner()
        {
            var route = new List<Vector2> { Vector2.zero, new Vector2(0f, -4f), new Vector2(4f, -4f) };
            var marks = RouteMarks.Along(route, 0f, 3f);
            Assert.AreEqual(3, marks.Count);
            Assert.AreEqual(new Vector2(0f, -3f), marks[1].Position);
            Assert.AreEqual(Vector2.right, marks[2].Direction);
            Assert.AreEqual(2f, marks[2].Position.x, 1e-4f);
        }

        [Test]
        public void NoSpacingMeansNoMarks()
        {
            var route = new List<Vector2> { Vector2.zero, Vector2.up };
            Assert.AreEqual(0, RouteMarks.Along(route, 0f, 0f).Count);
        }

        [Test]
        public void TheCurbExitRouteArrowsRunDownTheCorridorAndTurnEast()
        {
            var config = ScriptableObject.CreateInstance<LotConfig>();
            var geometry = new LotGeometry(config);
            var road = new RoadLayout(geometry, config.Site, Vector2.zero);
            var route = new List<Vector2>();
            road.CurbExitRoute(route);
            var marks = RouteMarks.Along(route, 2.4f, 2.4f);
            Assert.Greater(marks.Count, 4);
            Assert.IsTrue(marks.Exists(m => m.Direction.y < -0.99f && Mathf.Approximately(m.Position.x, geometry.ExitX)), "down the corridor");
            Assert.IsTrue(marks.Exists(m => m.Direction.x > 0.2f), "the turn east is marked");
            Object.DestroyImmediate(config);
        }
    }
}
