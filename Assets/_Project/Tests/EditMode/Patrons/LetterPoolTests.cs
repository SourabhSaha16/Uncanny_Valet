using NUnit.Framework;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class LetterPoolTests
    {
        [Test]
        public void HandsOutLowestFreeLetterFirst()
        {
            var pool = new LetterPool();
            Assert.AreEqual('A', pool.Acquire());
            Assert.AreEqual('B', pool.Acquire());
            Assert.AreEqual('C', pool.Acquire());
        }

        [Test]
        public void ReleasedLetterIsReusedBeforeHigherOnes()
        {
            var pool = new LetterPool();
            pool.Acquire(); pool.Acquire(); pool.Acquire();
            pool.Release('B');
            Assert.AreEqual('B', pool.Acquire());
            Assert.AreEqual('D', pool.Acquire());
        }

        [Test]
        public void ExhaustedPool_ReturnsOverflow()
        {
            var pool = new LetterPool();
            for (var i = 0; i < LetterPool.Count; i++) pool.Acquire();
            Assert.AreEqual(LetterPool.Overflow, pool.Acquire());
        }

        [Test]
        public void ReleasingUnknownCharacter_IsIgnored()
        {
            var pool = new LetterPool();
            Assert.DoesNotThrow(() => pool.Release(LetterPool.Overflow));
            Assert.AreEqual('A', pool.Acquire());
        }
    }
}
