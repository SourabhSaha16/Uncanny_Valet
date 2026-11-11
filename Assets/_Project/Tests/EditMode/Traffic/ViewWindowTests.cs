using NUnit.Framework;
using UncannyValet.Traffic;
using UnityEngine;

namespace UncannyValet.Tests.Traffic
{
    public class ViewWindowTests
    {
        private readonly ViewWindow _view = ViewWindow.Of(new Vector2(10f, 5f), 4f, 2f);

        [Test]
        public void OfACamera_HasTheRightRectangle()
        {
            Assert.AreEqual(2f, _view.Bounds.xMin, 1e-4f);
            Assert.AreEqual(18f, _view.Bounds.xMax, 1e-4f);
            Assert.AreEqual(1f, _view.Bounds.yMin, 1e-4f);
            Assert.AreEqual(9f, _view.Bounds.yMax, 1e-4f);
        }

        [Test]
        public void ContainsWithAMargin()
        {
            Assert.IsTrue(_view.Contains(new Vector2(10f, 5f), 0f));
            Assert.IsFalse(_view.Contains(new Vector2(19f, 5f), 0f));
            Assert.IsTrue(_view.Contains(new Vector2(19f, 5f), 1f));
        }

        [Test]
        public void ALaneIsCoveredWhenItsHeightIsInView()
        {
            Assert.IsTrue(_view.CoversHeight(1.5f, 0f));
            Assert.IsFalse(_view.CoversHeight(0.5f, 0f));
            Assert.IsTrue(_view.CoversHeight(0.5f, 1f));
        }

        [Test]
        public void UpstreamAndLeftDependOnTheDirection()
        {
            Assert.IsTrue(_view.IsUpstream(0f, false));
            Assert.IsFalse(_view.IsUpstream(0f, true));
            Assert.IsTrue(_view.IsUpstream(20f, true));
            Assert.IsTrue(_view.HasLeft(20f, false, 1f));
            Assert.IsFalse(_view.HasLeft(18.5f, false, 1f));
            Assert.IsTrue(_view.HasLeft(0f, true, 1f));
        }

        [Test]
        public void CarsEnterBeyondTheEdgeTheyApproachFrom()
        {
            Assert.AreEqual(0.6f, _view.EntryX(false, 1.4f), 1e-4f);
            Assert.AreEqual(19.4f, _view.EntryX(true, 1.4f), 1e-4f);
        }
    }
}
