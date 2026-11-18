using System;
using NUnit.Framework;
using UncannyValet.Bootstrap;
using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;
using UncannyValet.Tests.Lot;
using UnityEngine;

namespace UncannyValet.Tests.Integration
{
    /// <summary>The hand-made lot behind the bootstrap's lot seam (#194).</summary>
    public class HandmadeLotSourceTests
    {
        private HandmadeLot _lot;
        private LotConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LotConfig>();
            _lot = HandmadeSceneKit.Build(Vector2.zero, _config); // inactive: nothing is built yet
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_lot.gameObject);
            UnityEngine.Object.DestroyImmediate(_config);
        }

        [Test]
        public void Handmade_PassesTheLotThrough_AndSuppliesThePerfectSettings()
        {
            var source = LotSources.Handmade(_lot);
            Assert.AreSame(_lot, source.Layout);
            Assert.AreSame(_lot, source.Spots);
            Assert.AreSame(_lot.gameObject, source.Root);
            Assert.AreSame(_config.Perfect, source.Perfect, "Perfect Park thresholds come from the LotConfig on the lot root");
            Assert.IsNull(source.Road, "the road exists once the lot has woken up");
            Assert.IsNull(source.Paths);
        }

        [Test]
        public void ExtendView_LeavesTheRectAlone_UntilTheLotIsBuilt()
        {
            var rect = new Rect(1f, 2f, 30f, 40f);
            Assert.AreEqual(rect, LotSources.Handmade(_lot).ExtendView(rect));
        }

        [Test]
        public void Of_PrefersTheHandmadeLot_AndFallsBackToTheGeneratedOne()
        {
            var generated = new GameObject("Generated");
            try
            {
                var builder = generated.AddComponent<LotLayoutBuilder>();
                Assert.IsInstanceOf<HandmadeLotSource>(LotSources.Of(_lot, builder, null));
                Assert.IsInstanceOf<ParametricLotSource>(LotSources.Of(null, builder, null));
                Assert.IsNull(LotSources.Of(null, null, null));
            }
            finally { UnityEngine.Object.DestroyImmediate(generated); }
        }

        [Test]
        public void Handmade_WithoutALot_IsNull_AndTheConstructorRefuses()
        {
            Assert.IsNull(LotSources.Handmade(null));
            Assert.Throws<ArgumentNullException>(() => new HandmadeLotSource(null));
        }
    }
}
