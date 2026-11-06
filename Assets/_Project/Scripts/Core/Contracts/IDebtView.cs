namespace UncannyValet.Core.Contracts
{
    /// <summary>Read-only view of the debt. Implemented by the Economy module's ledger.</summary>
    public interface IDebtView
    {
        int DebtRemaining { get; }
        int TipsTonight { get; }
    }
}
