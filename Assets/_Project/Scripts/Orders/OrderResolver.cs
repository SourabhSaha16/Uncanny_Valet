using System;
using System.Collections.Generic;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;

namespace UncannyValet.Orders
{
    /// <summary>
    /// Ends orders. An order is judged only when its car is delivered to the patron (a failure also costs a
    /// strike). If the car's ticket ends any other way, or the night ends, the order is cancelled: no strike.
    /// </summary>
    public sealed class OrderResolver : IEventSubscriber
    {
        private readonly IEventBus _bus;
        private readonly OrderBook _book;
        private readonly OrderJudge _judge;
        private readonly StrikeTracker _strikes;
        private readonly List<Order> _open = new List<Order>();

        public OrderResolver(IEventBus bus, OrderBook book, OrderJudge judge, StrikeTracker strikes)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _book = book ?? throw new ArgumentNullException(nameof(book));
            _judge = judge ?? throw new ArgumentNullException(nameof(judge));
            _strikes = strikes ?? throw new ArgumentNullException(nameof(strikes));
        }

        public void Subscribe()
        {
            _bus.Subscribe<CarDelivered>(OnDelivered);
            _bus.Subscribe<TicketClosed>(OnTicketClosed);
            _bus.Subscribe<CarDeparted>(OnCarDeparted);
            _bus.Subscribe<NightEnded>(OnNightEnded);
        }

        public void Unsubscribe()
        {
            _bus.Unsubscribe<CarDelivered>(OnDelivered);
            _bus.Unsubscribe<TicketClosed>(OnTicketClosed);
            _bus.Unsubscribe<CarDeparted>(OnCarDeparted);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
        }

        private void OnTicketClosed(TicketClosed e) => Cancel(e.Car);
        private void OnCarDeparted(CarDeparted e) => Cancel(e.Car);

        private void OnDelivered(CarDelivered e)
        {
            if (!_book.TryGetByCar(e.Car, out var order)) return;
            _book.Remove(e.Car);
            var outcome = _judge.Judge(order);
            _bus.Publish(new OrderResolved(order.Id, order.Car, outcome));
            if (outcome == OrderOutcome.Failed) _strikes.AddStrike();
        }

        private void OnNightEnded(NightEnded e)
        {
            _open.Clear();
            _book.CopyTo(_open);
            for (var i = 0; i < _open.Count; i++) Cancel(_open[i].Car);
        }

        private void Cancel(CarId car)
        {
            if (!_book.TryGetByCar(car, out var order)) return;
            _book.Remove(car);
            _bus.Publish(new OrderResolved(order.Id, order.Car, OrderOutcome.Cancelled));
        }
    }
}
