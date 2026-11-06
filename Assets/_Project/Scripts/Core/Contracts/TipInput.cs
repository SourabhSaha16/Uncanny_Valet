namespace UncannyValet.Core.Contracts
{
    /// <summary>Everything the tip maths needs. Damage and requests join this struct later.</summary>
    public readonly struct TipInput
    {
        public readonly int BaseTip;
        public readonly float WaitMinutes;
        public readonly bool Impatient;

        public TipInput(int baseTip, float waitMinutes, bool impatient)
        {
            BaseTip = baseTip; WaitMinutes = waitMinutes; Impatient = impatient;
        }
    }
}
