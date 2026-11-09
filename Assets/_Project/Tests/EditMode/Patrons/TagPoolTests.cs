using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Config;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class TagPoolTests
    {
        [Test]
        public void HandsOutLowestFreeTagFirst()
        {
            var pool = new TagPool();
            Assert.AreEqual(0, pool.Acquire());
            Assert.AreEqual(1, pool.Acquire());
            Assert.AreEqual(2, pool.Acquire());
        }

        [Test]
        public void ReleasedTagIsReusedBeforeHigherOnes()
        {
            var pool = new TagPool();
            pool.Acquire(); pool.Acquire(); pool.Acquire();
            pool.Release(1);
            Assert.AreEqual(1, pool.Acquire());
        }

        [Test]
        public void NoTwoOpenTicketsShareATagWhileColoursRemain()
        {
            var pool = new TagPool();
            var seen = new HashSet<int>();
            for (var i = 0; i < TagPalette.Count; i++) Assert.IsTrue(seen.Add(pool.Acquire()));
        }

        [Test]
        public void WhenAllAreTaken_TheLeastUsedIsShared()
        {
            var pool = new TagPool();
            for (var i = 0; i < TagPalette.Count; i++) pool.Acquire();
            Assert.AreEqual(0, pool.Acquire());
            Assert.AreEqual(1, pool.Acquire());
        }

        [Test]
        public void ReleasingAnUnknownOrFreeTagIsHarmless()
        {
            var pool = new TagPool();
            Assert.DoesNotThrow(() => { pool.Release(-1); pool.Release(999); pool.Release(0); });
            Assert.AreEqual(0, pool.Acquire());
        }

        [Test]
        public void ResetFreesEverything()
        {
            var pool = new TagPool();
            pool.Acquire(); pool.Acquire();
            pool.Reset();
            Assert.AreEqual(0, pool.Acquire());
        }

        [Test]
        public void PaletteColoursAreDistinct()
        {
            Assert.GreaterOrEqual(TagPalette.Count, 8);
            var seen = new HashSet<UnityEngine.Color>();
            for (var i = 0; i < TagPalette.Count; i++) Assert.IsTrue(seen.Add(TagPalette.Get(i)), "colour " + i);
            Assert.AreEqual(TagPalette.Unknown, TagPalette.Get(-1));
            Assert.AreEqual(TagPalette.Get(0), TagPalette.Get(TagPalette.Count));
        }
    }
}
