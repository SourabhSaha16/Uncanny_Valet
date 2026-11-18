using System;
using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>The bootstrap's lot seam (#194): the parametric source passes the builder through, and registration puts the layout and bays in Services.</summary>
    public class LotSourceTests
    {
        private GameObject _go;
        private LotLayoutBuilder _lot;

        [SetUp]
        public void SetUp()
        {
            Services.Reset();
            _go = new GameObject("Lot");
            _go.SetActive(false); // the builder wakes up only when activated
            _lot = _go.AddComponent<LotLayoutBuilder>();
        }

        [TearDown]
        public void TearDown()
        {
            Services.Reset();
            UnityEngine.Object.DestroyImmediate(_go);
        }

        [Test]
        public void Parametric_PassesTheBuilderThrough()
        {
            var source = LotSources.Parametric(_lot, null);
            Assert.AreSame(_lot, source.Layout);
            Assert.AreSame(_lot, source.Spots);
            Assert.AreSame(_go, source.Root);
        }

        [Test]
        public void Parametric_WithoutDecor_HasNoPathsAndLeavesTheViewAlone()
        {
            var source = LotSources.Parametric(_lot, null);
            var rect = new Rect(1f, 2f, 30f, 40f);
            Assert.IsNull(source.Paths);
            Assert.AreEqual(rect, source.ExtendView(rect));
        }

        [Test]
        public void Parametric_WithoutBuilder_IsNull_AndTheConstructorRefuses()
        {
            Assert.IsNull(LotSources.Parametric(null, null));
            Assert.Throws<ArgumentNullException>(() => new ParametricLotSource(null, null));
        }

        [Test]
        public void RegisterLayout_RegistersTheLayoutAndTheBays()
        {
            LotRegistration.RegisterLayout(LotSources.Parametric(_lot, null));
            Assert.AreSame(_lot, Services.Get<ILotLayout>());
            Assert.AreSame(_lot, Services.Get<ISpotGeometry>());
        }
    }
}
