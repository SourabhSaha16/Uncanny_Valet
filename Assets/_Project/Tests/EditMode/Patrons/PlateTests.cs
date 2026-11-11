using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class PlateTests
    {
        private sealed class FixedRandom : IPatronRandom
        {
            private readonly float _value;
            public FixedRandom(float value) { _value = value; }
            public float Value() => _value;
            public float Range(float min, float max) => min + (max - min) * _value;
        }

        [Test]
        public void FirstAndLastIndexUseTheAlphabetEnds()
        {
            Assert.AreEqual("BB 0", PlateFormat.FromIndex(0));
            Assert.AreEqual("ZZ 9", PlateFormat.FromIndex(PlateFormat.Total - 1));
            Assert.AreEqual("BB 1", PlateFormat.FromIndex(1));
            Assert.AreEqual("BC 0", PlateFormat.FromIndex(PlateFormat.NumberCount));
        }

        [Test]
        public void IndexWrapsAround()
        {
            Assert.AreEqual(PlateFormat.FromIndex(0), PlateFormat.FromIndex(PlateFormat.Total));
            Assert.AreEqual(PlateFormat.FromIndex(PlateFormat.Total - 1), PlateFormat.FromIndex(-1));
        }

        [Test]
        public void AlphabetHasNoVowelsAndNoConfusingLetters()
        {
            Assert.AreEqual(PlateFormat.LetterCount, PlateFormat.Letters.Length);
            foreach (var bad in "AEIOUQY") Assert.AreEqual(-1, PlateFormat.Letters.IndexOf(bad), bad.ToString());
        }

        [Test]
        public void EveryGeneratedPlateHasTheShape()
        {
            var random = new SeededPatronRandom(7);
            var pool = new PlatePool(random);
            for (var i = 0; i < 300; i++) Assert.IsTrue(PlateFormat.IsValid(pool.Acquire()));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("A 1")]
        [TestCase("ABC 123")]
        [TestCase("KT4 ")]
        [TestCase("KQ 4")]
        [TestCase("KT 4A")]
        [TestCase("KT  4")]
        [TestCase("kt 4")]
        public void BadShapesAreRejected(string plate) => Assert.IsFalse(PlateFormat.IsValid(plate));

        [Test]
        public void SameSeedGivesTheSameSequence()
        {
            var a = new PlatePool(new SeededPatronRandom(99));
            var b = new PlatePool(new SeededPatronRandom(99));
            for (var i = 0; i < 20; i++) Assert.AreEqual(a.Acquire(), b.Acquire());
        }

        [Test]
        public void DifferentSeedsGiveDifferentPlates()
        {
            var a = new PlatePool(new SeededPatronRandom(1)).Acquire();
            var b = new PlatePool(new SeededPatronRandom(2)).Acquire();
            Assert.AreNotEqual(a, b);
        }

        [Test]
        public void ZeroSeedStillProducesValuesInRange()
        {
            var random = new SeededPatronRandom(0);
            for (var i = 0; i < 100; i++) Assert.That(random.Value(), Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
        }

        [Test]
        public void HeldPlatesAreUniqueEvenWhenTheRandomRepeats()
        {
            var pool = new PlatePool(new FixedRandom(0.5f));
            var seen = new HashSet<string>();
            for (var i = 0; i < 50; i++) Assert.IsTrue(seen.Add(pool.Acquire()));
            Assert.AreEqual(50, pool.Count);
        }

        [Test]
        public void ReleasedPlateCanBeIssuedAgain()
        {
            var pool = new PlatePool(new FixedRandom(0.25f));
            var first = pool.Acquire();
            var second = pool.Acquire();
            Assert.AreNotEqual(first, second);
            pool.Release(first);
            Assert.AreEqual(first, pool.Acquire());
        }

        [Test]
        public void ReleasingUnknownOrNullIsHarmless()
        {
            var pool = new PlatePool(new FixedRandom(0f));
            Assert.DoesNotThrow(() => { pool.Release(null); pool.Release("ZZ 9"); });
            Assert.AreEqual(0, pool.Count);
        }

        [Test]
        public void ResetFreesEveryPlate()
        {
            var pool = new PlatePool(new FixedRandom(0.1f));
            var first = pool.Acquire();
            pool.Acquire();
            pool.Reset();
            Assert.AreEqual(0, pool.Count);
            Assert.AreEqual(first, pool.Acquire());
        }

        [Test]
        public void ThereAre3610DistinctPlates()
        {
            Assert.AreEqual(3610, PlateFormat.Total);
            var seen = new HashSet<string>();
            for (var i = 0; i < PlateFormat.Total; i++) Assert.IsTrue(seen.Add(PlateFormat.FromIndex(i)));
        }

        [Test]
        public void ExhaustedPoolFallsBackToAValidPlateWithoutThrowing()
        {
            // A seeded source, not a fixed one: a constant draw makes every acquire probe past all held plates (about 6.5M string builds, 2.6 s).
            var pool = new PlatePool(new SeededPatronRandom(3));
            for (var i = 0; i < PlateFormat.Total; i++) pool.Acquire();
            Assert.AreEqual(PlateFormat.Total, pool.Count);
            string extra = null;
            Assert.DoesNotThrow(() => extra = pool.Acquire());
            Assert.IsTrue(PlateFormat.IsValid(extra));
        }

        [Test]
        public void ARandomAtTheTopOfTheRangeStaysInBounds()
        {
            var pool = new PlatePool(new FixedRandom(0.99999994f));
            Assert.IsTrue(PlateFormat.IsValid(pool.Acquire()));
        }
    }
}
