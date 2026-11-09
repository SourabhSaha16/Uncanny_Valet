using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Director;

namespace UncannyValet.Tests.Director
{
    /// <summary>Shared setup and helpers for the tracker fixtures.</summary>
    public abstract class TrackerTestBase
    {
        protected const int RegularSpots = 6;
        protected const int Bays = 2;

        protected EventBus Bus;
        protected FakeClock Clock;
        protected LotStateTracker Tracker;
        protected readonly CarId Car = new CarId(1);
        protected readonly TicketId Ticket = new TicketId(1);

        [SetUp]
        public void SetUp()
        {
            Bus = new EventBus();
            Clock = new FakeClock();
            Tracker = new LotStateTracker(Bus, Clock, new TuningStore(Bus), RegularSpots, Bays);
        }

        [TearDown]
        public void TearDown() => Tracker.Dispose();

        protected void Spawn(bool ev = false, bool dirty = false, float stay = 30f)
        {
            Bus.Publish(new CarSpawned(Car, Ticket));
            Bus.Publish(new TicketOpened(Ticket, Car, stay));
            Bus.Publish(new TicketTraitsAssigned(Ticket, Car, ev, dirty));
        }

        protected CarSnapshot Only() => Tracker.Capture().Cars[0];

        protected StationSlot Slot(StationKind kind, int index)
        {
            var seen = 0;
            var slots = Tracker.Capture().Slots;
            for (var i = 0; i < slots.Count; i++)
                if (slots[i].Kind == kind && seen++ == index) return slots[i];
            Assert.Fail("no such slot");
            return default;
        }
    }
}
