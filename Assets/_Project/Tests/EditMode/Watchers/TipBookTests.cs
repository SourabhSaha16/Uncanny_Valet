using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Watchers;
using UncannyValet.Watchers;

namespace UncannyValet.Tests.Watchers
{
    public sealed class TipBookTests
    {
        private const float Gap = 15f;
        private TipBook _book;

        [SetUp]
        public void SetUp()
        {
            _book = new TipBook(new List<WatcherTip>
            {
                new WatcherTip(TipSituation.MuddyCar, "WASH"),
                new WatcherTip(TipSituation.QueueFull, "QUEUE"),
                new WatcherTip(TipSituation.MuddyCar, "WASH TWO"),
            }, Gap);
        }

        [Test]
        public void PicksTheFirstTipOfTheSituation()
        {
            Assert.IsTrue(_book.TryPick(TipSituation.MuddyCar, 0f, out var index));
            Assert.AreEqual("WASH", _book.TextOf(index));
        }

        [Test]
        public void SituationWithoutATip_PicksNothing()
        {
            Assert.IsFalse(_book.TryPick(TipSituation.PatienceLow, 0f, out _));
        }

        [Test]
        public void ATipIsOnlyUsedWhenShown()
        {
            _book.TryPick(TipSituation.MuddyCar, 0f, out var first);
            _book.TryPick(TipSituation.MuddyCar, 1f, out var again);
            Assert.AreEqual(first, again, "a tip that was only requested stays available");
        }

        [Test]
        public void AShownTip_IsNeverPickedAgain()
        {
            _book.TryPick(TipSituation.MuddyCar, 0f, out var first);
            _book.MarkShown(first, 0f);
            Assert.IsTrue(_book.IsUsed(first));
            Assert.IsTrue(_book.TryPick(TipSituation.MuddyCar, 100f, out var next));
            Assert.AreEqual("WASH TWO", _book.TextOf(next));
            _book.MarkShown(next, 100f);
            Assert.IsFalse(_book.TryPick(TipSituation.MuddyCar, 200f, out _), "every tip of the situation was said");
        }

        [Test]
        public void TwoTips_AreAtLeastTheGapApart()
        {
            _book.TryPick(TipSituation.MuddyCar, 0f, out var first);
            _book.MarkShown(first, 10f);
            Assert.IsFalse(_book.TryPick(TipSituation.QueueFull, 24.9f, out _));
            Assert.IsTrue(_book.TryPick(TipSituation.QueueFull, 25f, out _));
        }
    }
}
