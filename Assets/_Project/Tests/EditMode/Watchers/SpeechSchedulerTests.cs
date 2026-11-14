using NUnit.Framework;
using UncannyValet.Core.Watchers;
using UncannyValet.Watchers;
using UnityEngine;

namespace UncannyValet.Tests.Watchers
{
    public sealed class SpeechSchedulerTests
    {
        private const float Bubble = 4f;
        private const float Gap = 8f;
        private SpeechScheduler _scheduler;

        [SetUp]
        public void SetUp() => _scheduler = new SpeechScheduler(Bubble, Gap);

        private static SpeechRequest Req(string text, SpeechPriority priority) => new SpeechRequest("X", text, Vector2.zero, null, priority);

        [Test]
        public void NothingPending_ShowsNothing()
        {
            Assert.IsFalse(_scheduler.Tick(0f, out _));
        }

        [Test]
        public void SingleRequest_IsShown()
        {
            _scheduler.Submit(Req("HI", SpeechPriority.Comment));
            Assert.IsTrue(_scheduler.Tick(1f, out var shown));
            Assert.AreEqual("HI", shown.Text);
        }

        [Test]
        public void CoincidingRequests_OnlyTheHighestPriorityShows()
        {
            _scheduler.Submit(Req("GAG", SpeechPriority.GagLine));
            _scheduler.Submit(Req("COMMENT", SpeechPriority.Comment));
            _scheduler.Submit(Req("TIP", SpeechPriority.Tip));
            _scheduler.Submit(Req("COMMENT 2", SpeechPriority.Comment));
            Assert.IsTrue(_scheduler.Tick(0f, out var shown));
            Assert.AreEqual("TIP", shown.Text);
        }

        [Test]
        public void CommentBeatsGagLine()
        {
            _scheduler.Submit(Req("GAG", SpeechPriority.GagLine));
            _scheduler.Submit(Req("COMMENT", SpeechPriority.Comment));
            _scheduler.Tick(0f, out var shown);
            Assert.AreEqual("COMMENT", shown.Text);
        }

        [Test]
        public void EqualPriorities_TheEarliestWins()
        {
            _scheduler.Submit(Req("FIRST", SpeechPriority.Comment));
            _scheduler.Submit(Req("SECOND", SpeechPriority.Comment));
            _scheduler.Tick(0f, out var shown);
            Assert.AreEqual("FIRST", shown.Text);
        }

        [Test]
        public void LosingRequests_AreDroppedNotQueued()
        {
            _scheduler.Submit(Req("TIP", SpeechPriority.Tip));
            _scheduler.Submit(Req("GAG", SpeechPriority.GagLine));
            _scheduler.Tick(0f, out _);
            Assert.IsFalse(_scheduler.Tick(20f, out _), "the dropped gag line must not come back later");
        }

        [Test]
        public void WhileABubbleShows_NewRequestsAreDropped()
        {
            _scheduler.Submit(Req("A", SpeechPriority.Comment));
            _scheduler.Tick(0f, out _);
            Assert.IsTrue(_scheduler.IsShowing(3.9f));
            _scheduler.Submit(Req("B", SpeechPriority.Tip));
            Assert.IsFalse(_scheduler.Tick(2f, out _), "one bubble at a time, even for a higher priority");
        }

        [Test]
        public void ABubbleLastsItsDuration()
        {
            _scheduler.Submit(Req("A", SpeechPriority.Comment));
            _scheduler.Tick(10f, out _);
            Assert.IsTrue(_scheduler.IsShowing(13.9f));
            Assert.IsFalse(_scheduler.IsShowing(14f));
        }

        [Test]
        public void AfterTheBubbleButInsideTheLockout_RequestsAreDropped()
        {
            _scheduler.Submit(Req("A", SpeechPriority.Comment));
            _scheduler.Tick(0f, out _);
            _scheduler.Submit(Req("B", SpeechPriority.Tip));
            Assert.IsFalse(_scheduler.Tick(7.9f, out _), "at most one bubble every 8 seconds");
        }

        [Test]
        public void AfterTheLockout_TheNextRequestShows()
        {
            _scheduler.Submit(Req("A", SpeechPriority.Comment));
            _scheduler.Tick(0f, out _);
            _scheduler.Submit(Req("B", SpeechPriority.GagLine));
            Assert.IsTrue(_scheduler.Tick(8f, out var shown));
            Assert.AreEqual("B", shown.Text);
        }

        [Test]
        public void Reset_ClearsLockoutsAndPending()
        {
            _scheduler.Submit(Req("A", SpeechPriority.Comment));
            _scheduler.Tick(0f, out _);
            _scheduler.Submit(Req("lost", SpeechPriority.Tip));
            _scheduler.Reset();
            Assert.IsFalse(_scheduler.IsShowing(1f));
            Assert.IsFalse(_scheduler.Tick(1f, out _), "pending requests are forgotten");
            _scheduler.Submit(Req("B", SpeechPriority.Comment));
            Assert.IsTrue(_scheduler.Tick(1f, out _));
        }
    }
}
