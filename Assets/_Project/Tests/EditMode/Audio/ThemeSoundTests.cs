using System;
using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Audio;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Core.Music;

namespace UncannyValet.Tests.Audio
{
    /// <summary>The escalation maths, the ambience and escalation loops, the new effects and their routing.</summary>
    public sealed class ThemeSoundTests
    {
        private const int Rate = 22050;

        private sealed class LotAudio : ILotAudio
        {
            public MusicThemeDefinition Theme { get; set; }
            public AmbienceSetDefinition Ambience { get; set; }
        }

        private static float Peak(float[] samples)
        {
            var peak = 0f;
            foreach (var s in samples) peak = Math.Max(peak, Math.Abs(s));
            return peak;
        }

        [Test]
        public void TheLastThirtySecondsRaiseTheTempoByTheRiseAndResetBringsItBack()
        {
            var rule = new EscalationRule(0.15f, 0f, 2f);
            Assert.AreEqual(1f, rule.Step(0.1f), 1e-5f);
            rule.Trigger();
            Assert.AreEqual(1.15f, rule.Step(0.1f), 1e-5f);
            rule.Reset();
            Assert.AreEqual(1f, rule.Step(0.1f), 1e-5f);
            Assert.IsFalse(rule.Active);
        }

        [Test]
        public void TheTempoEasesTowardTheRaisedValue()
        {
            var rule = new EscalationRule(0.15f, 2f, 2f);
            rule.Trigger();
            var first = rule.Step(0.5f);
            Assert.Greater(first, 1f);
            Assert.Less(first, 1.15f);
            for (var i = 0; i < 100; i++) rule.Step(0.5f);
            Assert.AreEqual(1.15f, rule.Tempo, 1e-3f);
        }

        [Test]
        public void TheExtraLayerAppearsAtTheEventAndFadesInOverItsRamp()
        {
            var rule = new EscalationRule(0.15f, 0f, 4f);
            Assert.AreEqual(0f, rule.LayerLevel);
            rule.Trigger();
            rule.Step(2f);
            Assert.AreEqual(0.5f, rule.LayerLevel, 1e-5f);
            rule.Step(10f);
            Assert.AreEqual(1f, rule.LayerLevel, 1e-5f);
        }

        [Test]
        public void EveryAmbienceLoopIsAudibleAndLoopsWithoutAJump()
        {
            foreach (AmbienceKind kind in Enum.GetValues(typeof(AmbienceKind)))
            {
                if (!AmbienceLoops.IsLoop(kind)) continue;
                var loop = AmbienceLoops.Build(kind, Rate);
                Assert.AreEqual(0.5f, Peak(loop), 1e-3f, kind.ToString());
                Assert.Less(Math.Abs(loop[0] - loop[loop.Length - 1]), 0.25f, kind + " seam");
            }
        }

        [Test]
        public void TheWashSplashIsNotALoop()
        {
            Assert.IsFalse(AmbienceLoops.IsLoop(AmbienceKind.WashSplash));
            Assert.Throws<ArgumentOutOfRangeException>(() => AmbienceLoops.Build(AmbienceKind.WashSplash, Rate));
        }

        [Test]
        public void EachEscalationLayerHasALoopAndNoneHasNone()
        {
            Assert.IsNull(EscalationLoops.Build(EscalationLayer.None, Rate));
            foreach (var layer in new[] { EscalationLayer.Fireworks, EscalationLayer.ThunderCloser })
                Assert.Greater(Peak(EscalationLoops.Build(layer, Rate)), 0.5f, layer.ToString());
        }

        [Test]
        public void TheNewEffectsAreAudibleAndAtMostASecondLong()
        {
            foreach (var id in new[] { SfxId.BlessingChime, SfxId.WashSplash, SfxId.MudSquelch })
            {
                var clip = SfxRecipeBook.Render(id, Rate);
                Assert.LessOrEqual(clip.Length, Rate, id.ToString());
                Assert.Greater(Peak(clip), 0.5f, id.ToString());
            }
        }

        [Test]
        public void NewSfxIdsAreAppendedAfterTheExistingNumbers()
        {
            Assert.AreEqual(27, (int)SfxId.BlessingChime);
            Assert.AreEqual(28, (int)SfxId.WashSplash);
            Assert.AreEqual(29, (int)SfxId.MudSquelch);
            Assert.AreEqual(26, (int)SfxId.PedestrianYelp);
        }

        [Test]
        public void AThemedLotPlaysTheChimeForEachBlessingAndAnUnthemedLotKeepsTheCoinSound()
        {
            using (var f = new ThemeFixtures())
            {
                var themed = Play(new LotAudio { Theme = f.Theme(voices: ThemeVoice.Shaker) }, new BlessingEarned(new TicketId(1), 5), new BlessingEarned(new TicketId(2), 3));
                CollectionAssert.AreEqual(new[] { SfxId.BlessingChime, SfxId.BlessingChime }, themed);
                var plain = Play(null, new BlessingEarned(new TicketId(1), 5));
                CollectionAssert.AreEqual(new[] { SfxId.BlessingEarned }, plain);
            }
        }

        [Test]
        public void EnteringMudPlaysTheSquelch()
        {
            CollectionAssert.AreEqual(new[] { SfxId.MudSquelch }, Play(null, new CarEnteredMud(new CarId(3))));
        }

        [Test]
        public void TheWashStationSplashesOnlyWhenTheAmbienceHasIt()
        {
            using (var f = new ThemeFixtures())
            {
                var splashy = new LotAudio { Ambience = f.Ambience(AmbienceKind.WaterwheelCreak, AmbienceKind.WashSplash) };
                var wash = new CarEnteredStation(new CarId(1), StationKind.Wash);
                CollectionAssert.AreEqual(new[] { SfxId.WashSplash }, Play(splashy, wash));
                CollectionAssert.AreEqual(new[] { SfxId.StationStart }, Play(null, wash));
                var charge = new CarEnteredStation(new CarId(1), StationKind.Charging);
                CollectionAssert.AreEqual(new[] { SfxId.StationStart }, Play(splashy, charge));
            }
        }

        private static List<SfxId> Play(ILotAudio lotAudio, params IGameEvent[] events)
        {
            var bus = new EventBus();
            var played = new List<SfxId>();
            var router = new SfxEventRouter(bus, played.Add, null, null, lotAudio);
            router.Subscribe();
            foreach (var e in events)
            {
                if (e is BlessingEarned blessing) bus.Publish(blessing);
                else if (e is CarEnteredMud mud) bus.Publish(mud);
                else if (e is CarEnteredStation station) bus.Publish(station);
            }
            router.Unsubscribe();
            return played;
        }
    }
}
