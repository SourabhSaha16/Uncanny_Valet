using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Audio;

namespace UncannyValet.Tests.Audio
{
    /// <summary>Level audio reset: a level change leaves exactly the new theme's layers active and everything else silenced.</summary>
    public sealed class LayerSetTests
    {
        private sealed class FakeLayer : IAudioLayer
        {
            public float Volume = 1f;
            public bool Released;
            public bool IsSilenced { get; private set; }

            public void Silence()
            {
                IsSilenced = true;
                Volume = 0f;
                Released = true;
            }
        }

        private static List<FakeLayer> Theme(int layers)
        {
            var list = new List<FakeLayer>();
            for (int i = 0; i < layers; i++) list.Add(new FakeLayer());
            return list;
        }

        private static void Switch(LayerSet set, List<FakeLayer> layers) => set.SwitchTo(layers.ConvertAll(l => (IAudioLayer)l));

        [Test]
        public void AChangeSilencesEveryLayerOfThePreviousTheme()
        {
            var set = new LayerSet();
            var old = Theme(3);
            Switch(set, old);
            var next = Theme(2);
            Switch(set, next);
            foreach (var layer in old)
            {
                Assert.IsTrue(layer.IsSilenced);
                Assert.AreEqual(0f, layer.Volume);
                Assert.IsTrue(layer.Released);
            }
            Assert.AreEqual(2, set.ActiveCount);
            foreach (var layer in next) Assert.IsFalse(layer.IsSilenced);
        }

        [Test]
        public void RepeatedChangesDoNotAccumulate()
        {
            var set = new LayerSet();
            var all = new List<FakeLayer>();
            for (int i = 0; i < 10; i++)
            {
                var theme = Theme(3);
                all.AddRange(theme);
                Switch(set, theme);
                Assert.AreEqual(3, set.ActiveCount);
            }
            Assert.AreEqual(27, all.FindAll(l => l.IsSilenced).Count);
        }

        [Test]
        public void ReplayingTheSameThemeRestartsCleanly()
        {
            var set = new LayerSet();
            var first = Theme(2);
            Switch(set, first);
            var again = Theme(2);
            Switch(set, again);
            Assert.AreEqual(2, set.ActiveCount);
            Assert.IsTrue(first.TrueForAll(l => l.IsSilenced));
        }

        [Test]
        public void SilenceAllLeavesNothingActiveAndIsRepeatable()
        {
            var set = new LayerSet();
            var theme = Theme(3);
            Switch(set, theme);
            set.SilenceAll();
            set.SilenceAll();
            Assert.AreEqual(0, set.ActiveCount);
            Assert.IsTrue(theme.TrueForAll(l => l.IsSilenced && l.Volume == 0f));
        }

        [Test]
        public void ALayerHeldAfterSilenceAllIsActive()
        {
            var set = new LayerSet();
            Switch(set, Theme(2));
            set.SilenceAll();
            set.Hold(new FakeLayer());
            Assert.AreEqual(1, set.ActiveCount);
        }
    }
}
