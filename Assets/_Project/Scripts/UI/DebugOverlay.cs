using System;
using System.Collections.Generic;
using System.Text;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.UI
{
    /// <summary>
    /// IMGUI debug overlay: clock, debt, tips, open tickets, interact prompt, pickup highlight.
    /// Ticket states come from events, so the overlay needs only Core. The prompt and clock format
    /// are supplied by the bootstrap as delegates.
    /// </summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        private readonly Dictionary<int, string> _states = new Dictionary<int, string>();
        private readonly Dictionary<int, int> _carOfTicket = new Dictionary<int, int>();
        private readonly StringBuilder _text = new StringBuilder();
        private Func<string> _prompt = () => string.Empty;
        private Func<float, string> _formatClock = m => ((int)m).ToString();
        private IEventBus _bus;
        private IGameClock _clock;
        private IDebtView _debt;
        private int _pickups;

        public void Configure(Func<string> prompt, Func<float, string> formatClock)
        {
            _prompt = prompt ?? _prompt;
            _formatClock = formatClock ?? _formatClock;
        }

        private void OnEnable()
        {
            _bus = Services.Get<IEventBus>();
            _clock = Services.Get<IGameClock>();
            _debt = Services.Get<IDebtView>();
            _bus.Subscribe<TicketOpened>(OnOpened);
            _bus.Subscribe<CarParked>(OnParked);
            _bus.Subscribe<PickupRequested>(OnPickup);
            _bus.Subscribe<TicketClosed>(OnClosed);
        }

        private void OnDisable()
        {
            _bus.Unsubscribe<TicketOpened>(OnOpened);
            _bus.Unsubscribe<CarParked>(OnParked);
            _bus.Unsubscribe<PickupRequested>(OnPickup);
            _bus.Unsubscribe<TicketClosed>(OnClosed);
        }

        private void OnOpened(TicketOpened e)
        {
            _states[e.Ticket.Value] = "AwaitingPark";
            _carOfTicket[e.Ticket.Value] = e.Car.Value;
        }

        private void OnParked(CarParked e)
        {
            foreach (var pair in _carOfTicket)
            {
                if (pair.Value != e.Car.Value || _states[pair.Key] != "AwaitingPark") continue;
                _states[pair.Key] = "Parked";
                return;
            }
        }

        private void OnPickup(PickupRequested e)
        {
            _states[e.Ticket.Value] = "PICKUP REQUESTED";
            _pickups++;
        }

        private void OnClosed(TicketClosed e)
        {
            if (_states.Remove(e.Ticket.Value) && _pickups > 0) _pickups--;
            _carOfTicket.Remove(e.Ticket.Value);
        }

        private void OnGUI()
        {
            if (_clock == null) return;
            _text.Clear();
            _text.AppendLine($"Night {_clock.Night}  {_formatClock(_clock.MinuteOfNight)}");
            _text.AppendLine($"Debt left: ${_debt.DebtRemaining}   Tips tonight: ${_debt.TipsTonight}");
            _text.AppendLine($"Open tickets: {_states.Count}");
            foreach (var pair in _states)
                _text.AppendLine($"  #{pair.Key} Car#{_carOfTicket[pair.Key]}: {pair.Value}");
            GUI.Label(new Rect(10, 10, 420, 400), _text.ToString());
            DrawPrompt();
            if (_pickups > 0) DrawPickupBanner();
        }

        private void DrawPrompt()
        {
            var prompt = _prompt();
            if (string.IsNullOrEmpty(prompt)) return;
            GUI.Box(new Rect(Screen.width * 0.5f - 90, Screen.height - 60, 180, 30), "[E] " + prompt);
        }

        private void DrawPickupBanner()
        {
            var old = GUI.color;
            GUI.color = Mathf.PingPong(Time.unscaledTime * 3f, 1f) > 0.5f ? Color.yellow : Color.white;
            GUI.Box(new Rect(Screen.width * 0.5f - 150, 10, 300, 30), "PICKUP REQUESTED: bring the car to the curb");
            GUI.color = old;
        }
    }
}
