using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Pure text rules for the night summary card. No Unity types.</summary>
    public static class SummaryCardFormatter
    {
        public const string NextLabel = "Next";
        public const string NewRunLabel = "New run";
        public const string PaidOffHeadline = "Debt paid off!";

        public static SummaryCardContent Build(NightSummary s)
        {
            return new SummaryCardContent
            {
                Night = s.Night,
                Title = "Night " + s.Night + " complete",
                Headline = s.IsFinalNight ? FinalHeadline(s) : string.Empty,
                Lines = new[]
                {
                    "Tips collected: " + Money(s.Tips),
                    "Cars parked: " + s.CarsParked,
                    "Cars not parked: " + s.CarsNotParked,
                    "Debt left: " + Money(s.DebtLeft),
                },
                ButtonLabel = s.IsFinalNight ? NewRunLabel : NextLabel,
            };
        }

        public static string FinalHeadline(NightSummary s)
        {
            return s.PaidOff ? PaidOffHeadline : "Debt left: " + Money(s.DebtLeft) + ": you did not pay it off";
        }

        public static string Money(int amount) => "$" + amount;
    }
}
