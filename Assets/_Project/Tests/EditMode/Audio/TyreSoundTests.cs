using System;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Tests.Audio
{
    public class TyreSoundTests
    {
        private const int Rate = 22050;
        private static SquealMix Mix() => new SquealMix(0.25f, 0.8f, 0.9f, 1.5f, 0.15f, 0.03f);

        [TestCase(SfxId.TyreChirp)]
        [TestCase(SfxId.TyreScreech)]
        public void OneShots_AreShortAndAudible(SfxId id)
        {
            var clip = SfxRecipeBook.Render(id, Rate);
            Assert.Less(clip.Length, Rate / 2);
            var peak = 0f;
            foreach (var s in clip) peak = Math.Max(peak, Math.Abs(s));
            Assert.Greater(peak, 0.1f);
            Assert.LessOrEqual(peak, 1f);
        }

        [Test]
        public void SquealLoop_IsAudibleWithinRangeAndDeterministic()
        {
            var loop = SquealLoop.Build(Rate);
            Assert.AreEqual((int)(Rate * SquealLoop.LoopSeconds), loop.Length);
            var peak = 0f;
            foreach (var s in loop) peak = Math.Max(peak, Math.Abs(s));
            Assert.Greater(peak, 0.5f);
            Assert.LessOrEqual(peak, 1f);
            CollectionAssert.AreEqual(loop, SquealLoop.Build(Rate));
        }

        [Test]
        public void SquealLoop_JoinsWithoutAClick()
        {
            var loop = SquealLoop.Build(Rate);
            var worstInside = 0f;
            for (var i = 1; i < loop.Length; i++) worstInside = Math.Max(worstInside, Math.Abs(loop[i] - loop[i - 1]));
            var seam = Math.Abs(loop[0] - loop[loop.Length - 1]);
            Assert.LessOrEqual(seam, worstInside, "the step across the wrap is no bigger than any step inside the loop");
        }

        // Mean absolute change per sample over mean absolute level: about the pitch of the sound times a constant.
        private static float Brightness(float[] s)
        {
            double diff = 0, level = 0;
            for (var i = 1; i < s.Length; i++) { diff += Math.Abs(s[i] - s[i - 1]); level += Math.Abs(s[i]); }
            return (float)(diff / Math.Max(level, 1e-6));
        }

        private static float CrossingsPerSecond(float[] s)
        {
            var n = 0;
            for (var i = 1; i < s.Length; i++) if (s[i - 1] < 0f != s[i] < 0f) n++;
            return n / ((float)s.Length / Rate);
        }

        [Test]
        public void SquealLoop_IsLowAndDull()
        {
            var loop = SquealLoop.Build(Rate);
            Assert.Less(CrossingsPerSecond(loop), 2000f, "fundamental 500 Hz = about 1000 crossings a second, not 2000+");
            Assert.Less(Brightness(loop), 0.35f);
        }

        [Test]
        public void Sharpness_MakesTheSquealAndTheOneShotsBrighter()
        {
            Assert.Less(Brightness(SquealLoop.Build(Rate, 0f)), Brightness(SquealLoop.Build(Rate, 1f)));
            Assert.Less(Brightness(SfxTyres.Chirp(Rate, 0f)), Brightness(SfxTyres.Chirp(Rate, 1f)));
            Assert.Less(Brightness(SfxTyres.Screech(Rate, 0f)), Brightness(SfxTyres.Screech(Rate, 1f)));
        }

        [Test]
        public void SquealLoop_StaysSeamlessAtEverySharpness()
        {
            foreach (var sharp in new[] { 0f, 0.35f, 1f })
            {
                var loop = SquealLoop.Build(Rate, sharp);
                var worst = 0f;
                for (var i = 1; i < loop.Length; i++) worst = Math.Max(worst, Math.Abs(loop[i] - loop[i - 1]));
                Assert.LessOrEqual(Math.Abs(loop[0] - loop[loop.Length - 1]), worst * 1.2f, "sharpness " + sharp);
            }
        }

        [Test]
        public void OneShotsAndBumps_AreSoftAtTheDefault()
        {
            Assert.Less(Brightness(SfxRecipeBook.Render(SfxId.TyreChirp, Rate)), 0.25f);
            Assert.Less(Brightness(SfxRecipeBook.Render(SfxId.TyreScreech, Rate)), 0.25f);
            Assert.Less(Brightness(SfxRecipeBook.Render(SfxId.CarBump, Rate)), 0.2f);
            Assert.Less(Brightness(SfxRecipeBook.Render(SfxId.PedestrianBump, Rate)), 0.2f);
        }

        [Test]
        public void SoftenFilter_CutoffRunsFromDullToBright()
        {
            Assert.AreEqual(SoftenFilter.MinCutoffHz, SoftenFilter.CutoffFor(0f));
            Assert.AreEqual(SoftenFilter.MaxCutoffHz, SoftenFilter.CutoffFor(1f));
            Assert.AreEqual(SoftenFilter.MinCutoffHz, SoftenFilter.CutoffFor(-3f));
        }

        [Test]
        public void TyreSoundTuning_StartsWithTheGivenValuesAndFollowsTheKeys()
        {
            var bus = new EventBus();
            var changes = 0;
            var tuning = new TyreSoundTuning(bus, 1f, 0.35f, () => changes++);
            tuning.Subscribe();
            bus.Publish(new TuningChanged(TuningKey.TyreSoundSharpness, 0.8f));
            bus.Publish(new TuningChanged(TuningKey.TyreSoundVolume, 0.5f));
            bus.Publish(new TuningChanged(TuningKey.BaseTip, 3f));
            Assert.AreEqual(0.8f, tuning.Sharpness, 1e-5f);
            Assert.AreEqual(0.5f, tuning.Volume, 1e-5f);
            Assert.AreEqual(2, changes);
            tuning.Unsubscribe();
            bus.Publish(new TuningChanged(TuningKey.TyreSoundVolume, 0.1f));
            Assert.AreEqual(0.5f, tuning.Volume, 1e-5f);
        }

        [Test]
        public void Mix_VolumeAndPitchRiseWithSlipIntensity()
        {
            var gentle = Mix();
            var full = Mix();
            for (var i = 0; i < 10; i++) { gentle.Step(true, 0f, 0.05f); full.Step(true, 1f, 0.05f); }
            Assert.AreEqual(0.25f, gentle.Volume, 1e-5f);
            Assert.AreEqual(0.8f, full.Volume, 1e-5f);
            Assert.AreEqual(0.9f, gentle.Pitch, 1e-5f);
            Assert.AreEqual(1.5f, full.Pitch, 1e-5f);
        }

        [Test]
        public void Mix_FadesToSilenceWithinTheFadeTimeAndHoldsItsPitch()
        {
            var mix = Mix();
            for (var i = 0; i < 10; i++) mix.Step(true, 1f, 0.05f);
            mix.Step(false, 0f, 0.075f);
            Assert.AreEqual(0.4f, mix.Volume, 1e-4f, "half way after half the fade time");
            mix.Step(false, 0f, 0.075f);
            Assert.AreEqual(0f, mix.Volume, 1e-5f);
            Assert.AreEqual(1.5f, mix.Pitch, 1e-5f);
        }

        [Test]
        public void Mix_AttackIsQuickButNotInstant()
        {
            var mix = Mix();
            mix.Step(true, 1f, 0.01f);
            Assert.Greater(mix.Volume, 0f);
            Assert.Less(mix.Volume, 0.8f);
        }

        [Test]
        public void Router_ChirpsOnSlideStartAndScreechesOnHardBrakeWithRateLimits()
        {
            var bus = new EventBus();
            var played = new List<SfxId>();
            var now = 0f;
            var router = new SfxEventRouter(bus, played.Add, () => now);
            router.Subscribe();
            var car = new CarId(1);
            bus.Publish(new CarSlideStarted(car));
            bus.Publish(new CarSlideStarted(car));
            bus.Publish(new CarHardBraked(car));
            bus.Publish(new CarHardBraked(car));
            bus.Publish(new CarSlideEnded(car));
            CollectionAssert.AreEqual(new[] { SfxId.TyreChirp, SfxId.TyreScreech }, played);
            now += SfxEventRouter.ScreechMinInterval;
            bus.Publish(new CarSlideStarted(car));
            bus.Publish(new CarHardBraked(car));
            Assert.AreEqual(4, played.Count);
            router.Unsubscribe();
            bus.Publish(new CarSlideStarted(car));
            Assert.AreEqual(4, played.Count);
        }
    }
}
