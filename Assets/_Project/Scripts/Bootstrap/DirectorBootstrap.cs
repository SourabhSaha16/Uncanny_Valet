using System;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;

namespace UncannyValet.Bootstrap
{
    /// <summary>Creates and registers the Patron Director's plain parts (lot tracker, view) and activates its shell.</summary>
    public sealed class DirectorBootstrap : IDisposable
    {
        private readonly LotStateTracker _tracker;

        private DirectorBootstrap(LotStateTracker tracker) { _tracker = tracker; }

        /// <summary>Registers ILotSnapshotSource and IDirectorView, then initialises and activates the director.</summary>
        public static DirectorBootstrap Start(IEventBus bus, ITuning tuning, IGameClock clock, ILotLayout lot,
            PatronDirector director, DirectorProfile profile, IDirectorRandom random, int maxCombo)
        {
            var bays = 0;
            for (var i = 0; i < lot.SpotIds.Count; i++)
                if (lot.IsChargingSpot(lot.SpotIds[i])) bays++;
            var tracker = new LotStateTracker(bus, clock, tuning, lot.SpotIds.Count - bays, bays);
            var state = new DirectorState();
            Services.Register<ILotSnapshotSource>(tracker);
            Services.Register<IDirectorView>(state);
            director.Initialize(profile, state, random, maxCombo);
            director.gameObject.SetActive(true);
            return new DirectorBootstrap(tracker);
        }

        public void Dispose() => _tracker.Dispose();
    }
}
