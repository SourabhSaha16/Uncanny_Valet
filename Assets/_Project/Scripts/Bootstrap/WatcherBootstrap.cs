using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Pedestrians;
using UncannyValet.Run;
using UncannyValet.Watchers;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Starts the speech system of a lot: always the speech director (floating gag lines need it in every lot) and, when the lot names a
    /// watcher, the character standing at the lot's watcher spot with its tips and comments. Registers <see cref="IWatcherSource"/>.
    /// </summary>
    public static class WatcherBootstrap
    {
        private const string BodyName = "Watcher";
        private const string HostName = "Speech";

        /// <summary>Needs IEventBus registered. <paramref name="origin"/> is the lot's origin, to which the spot is relative.</summary>
        public static void Start(LotDefinition lot, WatcherConfig config, PedestrianCrowd crowd, Transform origin)
        {
            Services.Register<IWatcherSource>(new WatcherSource(lot.Watcher));
            var anchor = lot.Watcher != null ? CreateBody(lot, crowd, origin) : null;
            var host = new GameObject(HostName);
            host.SetActive(false); // configured before OnEnable runs
            host.AddComponent<WatcherHost>().Configure(config, lot.Watcher, anchor, lot.QueueSlots);
            host.SetActive(true);
        }

        private static Transform CreateBody(LotDefinition lot, PedestrianCrowd crowd, Transform origin)
        {
            var go = new GameObject(BodyName + " " + lot.Watcher.SpeakerName);
            go.transform.position = (Vector2)origin.position + lot.WatcherSpot;
            var body = go.AddComponent<WatcherBody>();
            body.Build(lot.Watcher.Look, crowd != null ? crowd.Config : null);
            return go.transform;
        }
    }
}
