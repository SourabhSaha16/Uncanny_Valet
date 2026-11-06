namespace UncannyValet.Core.Contracts
{
    /// <summary>Pure tip maths. Never returns less than 0. Implemented by the Economy module.</summary>
    public interface ITipCalculator
    {
        int Calculate(in TipInput input);
    }
}
