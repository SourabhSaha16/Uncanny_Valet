using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Economy;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Owns the run's <see cref="DebtLedger"/> and exposes it as the one <see cref="IDebtView"/> everyone holds.
    /// A new run swaps in a fresh ledger at the starting debt, so readers never see a stale instance.
    /// </summary>
    public sealed class RunDebt : IDebtView, IRunReset, IDisposable
    {
        private readonly IEventBus _bus;
        private readonly int _startingDebt;
        private DebtLedger _ledger;

        public int DebtRemaining => _ledger.DebtRemaining;
        public int TipsTonight => _ledger.TipsTonight;

        public RunDebt(IEventBus bus, int startingDebt)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _startingDebt = startingDebt;
            _ledger = new DebtLedger(bus, startingDebt);
        }

        public void ResetRun()
        {
            _ledger.Dispose();
            _ledger = new DebtLedger(_bus, _startingDebt);
            _bus.Publish(new DebtChanged(DebtRemaining, TipsTonight));
        }

        public void Dispose() => _ledger.Dispose();
    }
}
