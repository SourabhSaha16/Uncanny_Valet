using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UnityEngine;
using UnityEngine.TestTools;

namespace UncannyValet.Tests.PlayMode
{
    /// <summary>The Storm wind reward of the crazy Vanaheim lot: the power is recorded, but its key does nothing yet (its effect arrives with task 89) and spends no bucket.</summary>
    public sealed class VanaheimStormWindPlayModeTests : VanaheimPlayBase
    {
        private sealed class PressedKey : IPowerInput
        {
            public int Slot = -1;
            public bool PowerPressed(int slot) => slot == Slot;
        }

        [UnityTest]
        public IEnumerator WithStormWindOwned_Key2DoesNothing_AndSpendsNoBucket()
        {
            yield return Play(CrazyPath);
            var key = new PressedKey();
            var activation = Object.FindFirstObjectByType<PowerActivationController>();
            typeof(PowerActivationController).GetField("_input", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(activation, key);
            var bar = Object.FindFirstObjectByType<PowerBarController>();
            var bus = Services.Get<IEventBus>();
            Services.Get<ITuning>().Set(TuningKey.DevOwnThunderStride, 1f);
            bus.Publish(new PowerGranted(StoryPower.StormWind, 1)); // what clearing the crazy lot does
            var progress = Services.Get<IRunProgress>();
            Assert.AreEqual(1, progress.PowerLevel(StoryPower.StormWind));
            CollectionAssert.AreEqual(new[] { StoryPower.ThunderStride, StoryPower.StormWind }, progress.EarnedPowers);
            var activated = new List<PowerActivated>();
            bus.Subscribe<PowerActivated>(activated.Add);
            bus.Publish(new PerfectPark(new CarId(1), TicketId.None));
            bus.Publish(new PerfectPark(new CarId(2), TicketId.None));
            yield return null;
            var open = bar.Bar.OpenBuckets;
            var value = bar.Bar.Value;
            Assert.Greater(open, 0, "a bucket is open");
            key.Slot = 1; // key 2
            yield return null;
            yield return null;
            key.Slot = -1;
            Assert.IsEmpty(activated, "Storm wind has no effect yet");
            Assert.AreEqual(open, bar.Bar.OpenBuckets);
            Assert.AreEqual(value, bar.Bar.Value, 0.001f, "no bucket is spent");
            Assert.IsEmpty(Problems, string.Join("\n", Problems));
        }
    }
}
