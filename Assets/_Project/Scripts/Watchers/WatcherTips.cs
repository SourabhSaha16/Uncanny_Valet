using System;
using UncannyValet.Core.Events;
using UncannyValet.Core.Modifiers;
using UncannyValet.Core.Watchers;

namespace UncannyValet.Watchers
{
    /// <summary>
    /// Makes a watcher say a tip about the lot's mechanics when a situation arises: the first arrival, a Muddy or a charging car, a patron's
    /// patience nearly gone, a full queue, a damaged car. The <see cref="TipBook"/> allows each tip once and spaces them out; a tip counts as
    /// said only when the speech director really showed it. Never about powers.
    /// </summary>
    public sealed class WatcherTips : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly WatcherVoice _voice;
        private readonly TipBook _book;
        private readonly float _patienceNearlyGone;
        private readonly int _queueSlots;
        private readonly Func<int> _waitingCars;
        private readonly Func<float> _now;
        private int _pending = -1;

        /// <param name="waitingCars">How many cars wait in the arrival queue right now.</param>
        /// <param name="now">Real time in seconds.</param>
        public WatcherTips(IEventBus bus, WatcherVoice voice, WatcherTextAsset text, WatcherConfig config, int queueSlots, Func<int> waitingCars, Func<float> now)
        {
            _bus = bus; _voice = voice; _queueSlots = queueSlots; _waitingCars = waitingCars; _now = now;
            _book = new TipBook(text.Tips, config.TipGapSeconds);
            _patienceNearlyGone = config.PatienceNearlyGone;
            _bus.Subscribe<TicketOpened>(OnTicketOpened);
            _bus.Subscribe<TicketModifiersAssigned>(OnModifiers);
            _bus.Subscribe<TicketTimerChanged>(OnTimer);
            _bus.Subscribe<ShiftTick>(OnTick);
            _bus.Subscribe<CarDamaged>(OnDamaged);
            _bus.Subscribe<SpeechShown>(OnShown);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TicketOpened>(OnTicketOpened);
            _bus.Unsubscribe<TicketModifiersAssigned>(OnModifiers);
            _bus.Unsubscribe<TicketTimerChanged>(OnTimer);
            _bus.Unsubscribe<ShiftTick>(OnTick);
            _bus.Unsubscribe<CarDamaged>(OnDamaged);
            _bus.Unsubscribe<SpeechShown>(OnShown);
        }

        private void OnTicketOpened(TicketOpened e) => Offer(TipSituation.FirstArrival);

        private void OnDamaged(CarDamaged e) => Offer(TipSituation.CarDamaged);

        private void OnModifiers(TicketModifiersAssigned e)
        {
            if (e.Modifiers.Contains(ModifierId.Muddy)) Offer(TipSituation.MuddyCar);
            if (e.Modifiers.Contains(ModifierId.NeedsCharge)) Offer(TipSituation.ChargeCar);
        }

        private void OnTimer(TicketTimerChanged e)
        {
            if (SituationRules.IsPatienceNearlyGone(e.Elapsed01, _patienceNearlyGone)) Offer(TipSituation.PatienceLow);
        }

        private void OnTick(ShiftTick e)
        {
            if (SituationRules.IsQueueFull(_waitingCars(), _queueSlots)) Offer(TipSituation.QueueFull);
        }

        // Asks to say the next unused tip of the situation; the director may still drop it.
        private void Offer(TipSituation situation)
        {
            if (!_book.TryPick(situation, _now(), out var index)) return;
            _pending = index;
            _voice.Say(_book.TextOf(index), SpeechPriority.Tip);
        }

        private void OnShown(SpeechShown e)
        {
            if (_pending < 0 || e.Request.Priority != SpeechPriority.Tip || e.Request.Speaker != _voice.Speaker) return;
            if (e.Request.Text != _book.TextOf(_pending)) return;
            _book.MarkShown(_pending, _now());
            _pending = -1;
        }
    }
}
