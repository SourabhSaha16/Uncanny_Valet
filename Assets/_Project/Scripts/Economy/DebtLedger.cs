using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Economy
{
    /// <summary>Tracks tonight's tips and the debt. Tips only reach the debt when the night ends.</summary>
    public sealed class DebtLedger : IDebtView, IDisposable
    {
        private readonly IEventBus _bus;

        public int DebtRemaining { get; private set; }
        public int TipsTonight { get; private set; }

        public DebtLedger(IEventBus bus, int startingDebt)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            DebtRemaining = Math.Max(0, startingDebt);
            _bus.Subscribe<TipEarned>(OnTipEarned);
            _bus.Subscribe<NightEnded>(OnNightEnded);
        }

        public void Dispose()
        {
            _bus.Unsubscribe<TipEarned>(OnTipEarned);
            _bus.Unsubscribe<NightEnded>(OnNightEnded);
        }

        private void OnTipEarned(TipEarned e)
        {
            TipsTonight += Math.Max(0, e.Amount);
            Publish();
        }

        private void OnNightEnded(NightEnded e)
        {
            DebtRemaining = Math.Max(0, DebtRemaining - TipsTonight);
            TipsTonight = 0;
            Publish();
        }

        private void Publish() => _bus.Publish(new DebtChanged(DebtRemaining, TipsTonight));
    }
}
