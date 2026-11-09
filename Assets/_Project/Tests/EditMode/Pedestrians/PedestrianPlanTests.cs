using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Tests.Pedestrians
{
    public class PedestrianPlanTests
    {
        [Test]
        public void SameIndexAlwaysGivesTheSameLook()
        {
            var a = PedestrianPlan.Look(5, 6, 8, 6);
            var b = PedestrianPlan.Look(5, 6, 8, 6);
            Assert.AreEqual(a.Skin, b.Skin);
            Assert.AreEqual(a.Hair, b.Hair);
            Assert.AreEqual(a.Outfit, b.Outfit);
        }

        [Test]
        public void LooksStayInsideTheAvailableRows()
        {
            for (var i = 0; i < 50; i++)
            {
                var look = PedestrianPlan.Look(i, 6, 8, 6);
                Assert.That(look.Skin, Is.InRange(0, 5));
                Assert.That(look.Hair, Is.InRange(0, 7));
                Assert.That(look.Outfit, Is.InRange(0, 5));
            }
        }

        [Test]
        public void TheFirstSixPedestriansWearDifferentOutfitsAndSkins()
        {
            var outfits = new HashSet<int>();
            var skins = new HashSet<int>();
            for (var i = 0; i < 6; i++)
            {
                var look = PedestrianPlan.Look(i, 6, 8, 6);
                outfits.Add(look.Outfit);
                skins.Add(look.Skin);
            }
            Assert.AreEqual(6, outfits.Count);
            Assert.AreEqual(6, skins.Count);
        }

        [Test]
        public void TheFirstEightPedestriansHaveDifferentHair()
        {
            var hairs = new HashSet<int>();
            for (var i = 0; i < 8; i++) hairs.Add(PedestrianPlan.Look(i, 6, 8, 6).Hair);
            Assert.AreEqual(8, hairs.Count);
        }

        [Test]
        public void StrideIsNudgedUntilItIsCoprime()
        {
            Assert.AreEqual(5, PedestrianPlan.CoprimeStride(6, 5));
            Assert.AreEqual(7, PedestrianPlan.CoprimeStride(6, 6));
            Assert.AreEqual(1, PedestrianPlan.CoprimeStride(1, 0));
            Assert.AreEqual(0, PedestrianPlan.Pick(3, 0, 3, 0));
        }

        [Test]
        public void PedestriansAreDealtOutToTheRoutesInTurn()
        {
            Assert.AreEqual(0, PedestrianPlan.RouteFor(0, 5));
            Assert.AreEqual(4, PedestrianPlan.RouteFor(4, 5));
            Assert.AreEqual(0, PedestrianPlan.RouteFor(5, 5));
            Assert.AreEqual(-1, PedestrianPlan.RouteFor(3, 0));
        }

        [Test]
        public void WalkersSharingARouteStartAtDifferentPlaces()
        {
            var starts = new HashSet<float>();
            for (var i = 0; i < 10; i++)
                if (PedestrianPlan.RouteFor(i, 5) == 2) starts.Add(PedestrianPlan.StartFraction(i, 5, 10));
            Assert.AreEqual(2, starts.Count);
        }

        [Test]
        public void StartFractionIsAlwaysInsideTheCycle()
        {
            for (var i = 0; i < 10; i++)
                Assert.That(PedestrianPlan.StartFraction(i, 7, 10), Is.InRange(0f, 0.9999f));
            Assert.AreEqual(0f, PedestrianPlan.StartFraction(0, 0, 10));
        }

        [Test]
        public void SpeedStaysWithinTheConfiguredRangeAndVaries()
        {
            var speeds = new HashSet<float>();
            for (var i = 0; i < 10; i++)
            {
                var speed = PedestrianPlan.Speed(i, 0.9f, 1.5f);
                Assert.That(speed, Is.InRange(0.9f, 1.5f));
                speeds.Add(Mathf.Round(speed * 1000f));
            }
            Assert.Greater(speeds.Count, 3);
        }
    }
}
