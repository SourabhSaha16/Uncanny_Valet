using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Watchers;
using UncannyValet.Watchers;
using UnityEngine;

namespace UncannyValet.Tests.Watchers
{
    /// <summary>Shared helpers for the watcher tests: a bus that records every speech request, and small text assets.</summary>
    public sealed class WatcherTestKit
    {
        public readonly EventBus Bus = new EventBus();
        public readonly List<SpeechRequest> Requests = new List<SpeechRequest>();
        public readonly List<SpeechShown> Shown = new List<SpeechShown>();

        public WatcherTestKit()
        {
            Bus.Subscribe<SpeechRequested>(e => Requests.Add(e.Request));
            Bus.Subscribe<SpeechShown>(e => Shown.Add(e));
        }

        public WatcherVoice Voice() => new WatcherVoice(Bus, "TESTER", Vector2.zero, null);

        /// <summary>A text asset with one tip per situation and two lines per event.</summary>
        public static WatcherTextAsset Text()
        {
            var tips = new List<WatcherTip>();
            foreach (TipSituation s in System.Enum.GetValues(typeof(TipSituation))) tips.Add(new WatcherTip(s, "TIP " + s));
            var comments = new Dictionary<WatcherEvent, string[]>();
            foreach (WatcherEvent e in System.Enum.GetValues(typeof(WatcherEvent))) comments[e] = new[] { e + " ONE", e + " TWO" };
            return WatcherTextAsset.Create("TestText", tips, comments, null);
        }
    }
}
