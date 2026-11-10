using System.Collections.Generic;
using NUnit.Framework;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UncannyValet.Patrons;

namespace UncannyValet.Tests.Patrons
{
    public class TicketPlateTests
    {
        private sealed class FakeClock : IGameClock
        {
            public float SecondsRemaining => 0f;
            public float ShiftLengthSeconds => 0f;
            public float ElapsedMinutes { get; set; }
            public bool IsRunning => true;
        }

        private sealed class FakeTips : ITipCalculator
        {
            public int Calculate(in TipInput input) => 25;
            public TipBreakdown Explain(in TipInput input) => new TipBreakdown(25, 0, 0, 0, 25, TipZeroReason.None);
        }

        private sealed class ConstantRandom : IPatronRandom
        {
            public float Value() => 0.3f;
            public float Range(float min, float max) => min;
        }

        private EventBus _bus;
        private TicketRegistry _registry;
        private List<CarPlateAssigned> _plates;
        private List<IGameEvent> _order;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _registry = new TicketRegistry(_bus, new FakeClock(), new FakeTips(), ServiceTuning.Default, null,
                new PlatePool(new ConstantRandom()));
            _plates = new List<CarPlateAssigned>();
            _order = new List<IGameEvent>();
            _bus.Subscribe<CarPlateAssigned>(e => { _plates.Add(e); _order.Add(e); });
            _bus.Subscribe<CarTagAssigned>(e => _order.Add(e));
            _bus.Subscribe<TicketOpened>(e => _order.Add(e));
        }

        private Ticket Arrive(int car)
        {
            var ticket = _registry.Create(new CarId(car), new PatronRequest(20f, 20, false, 60f));
            _registry.AnnounceArrival(ticket);
            return ticket;
        }

        [Test]
        public void EveryTicketGetsAValidPlateAndOpenPlatesAreUnique()
        {
            var seen = new HashSet<string>();
            for (var car = 1; car <= 6; car++)
            {
                var plate = Arrive(car).Plate;
                Assert.IsTrue(PlateFormat.IsValid(plate), plate);
                Assert.IsTrue(seen.Add(plate), plate);
            }
        }

        [Test]
        public void AnnouncementCarriesTheTicketsPlate()
        {
            var ticket = Arrive(1);
            Assert.AreEqual(1, _plates.Count);
            Assert.AreEqual(ticket.CarId, _plates[0].Car);
            Assert.AreEqual(ticket.Plate, _plates[0].Plate);
        }

        [Test]
        public void PlateIsAnnouncedAfterTheTicketOpensAndTheTag()
        {
            Arrive(1);
            Assert.IsInstanceOf<TicketOpened>(_order[0]);
            Assert.IsInstanceOf<CarTagAssigned>(_order[1]);
            Assert.IsInstanceOf<CarPlateAssigned>(_order[2]);
        }

        [Test]
        public void ClosingATicketReleasesItsPlate()
        {
            var first = Arrive(1);
            var plate = first.Plate;
            _bus.Publish(new ShiftEnded());
            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(plate, Arrive(2).Plate);
        }

        [Test]
        public void ShiftEndReleasesAnUnparkedTicketsPlate()
        {
            var first = Arrive(1);
            var plate = first.Plate;
            _bus.Publish(new ShiftEnded());
            Assert.AreEqual(0, _registry.OpenCount);
            Assert.AreEqual(plate, Arrive(2).Plate);
        }
    }
}
