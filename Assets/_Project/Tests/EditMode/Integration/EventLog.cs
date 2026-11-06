using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Tests.Integration
{
    /// <summary>Records every game event published on a bus, in order.</summary>
    public sealed class EventLog
    {
        private readonly List<IGameEvent> _events = new List<IGameEvent>();

        public EventLog(IEventBus bus)
        {
            Add<CarSpawned>(bus); Add<CarEntered>(bus); Add<CarExited>(bus); Add<CarParked>(bus);
            Add<CarLeftSpot>(bus); Add<CarAtCurb>(bus); Add<CarDeparted>(bus);
            Add<TicketOpened>(bus); Add<PickupRequested>(bus); Add<TicketClosed>(bus);
            Add<TipEarned>(bus); Add<DebtChanged>(bus);
            Add<NightStarted>(bus); Add<NightEnded>(bus);
        }

        public IReadOnlyList<IGameEvent> All => _events;

        public int Count<T>() where T : struct, IGameEvent
        {
            var n = 0;
            foreach (var e in _events) if (e is T) n++;
            return n;
        }

        public T Last<T>() where T : struct, IGameEvent
        {
            for (var i = _events.Count - 1; i >= 0; i--) if (_events[i] is T hit) return hit;
            throw new InvalidOperationException("No " + typeof(T).Name + " logged.");
        }

        public int IndexOf<T>() where T : struct, IGameEvent
        {
            for (var i = 0; i < _events.Count; i++) if (_events[i] is T) return i;
            return -1;
        }

        private void Add<T>(IEventBus bus) where T : struct, IGameEvent => bus.Subscribe<T>(e => _events.Add(e));
    }
}
