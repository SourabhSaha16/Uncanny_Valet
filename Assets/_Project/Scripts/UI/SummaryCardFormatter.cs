using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.UI
{
    /// <summary>Pure text rules for the night summary card. No Unity types.</summary>
    public static class SummaryCardFormatter
    {
        public const string NextLabel = "Next";
        public const string NewRunLabel = "New run";
        public const string PaidOffHeadline = "Debt paid off!";
        public const string CalcHeader = "Tip calculation";
        public const string TomorrowPrefix = "Tomorrow: ";

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
                CalcLines = CalcLines(s),
                Tomorrow = TomorrowText(s),
                ButtonLabel = s.IsFinalNight ? NewRunLabel : NextLabel,
            };
        }

        public static string[] CalcLines(NightSummary s)
        {
            var rows = new List<string>(6) { "Base tips: " + Money(s.BaseTips), SpeedLine(s.SpeedAdjust) };
            if (s.ChargeBonus != 0 || s.CleanBonus != 0)
            {
                rows.Add("Charge bonus: " + Signed(s.ChargeBonus));
                rows.Add("Clean bonus: " + Signed(s.CleanBonus));
            }
            rows.Add("= Tips collected: " + Money(s.Tips));
            if (s.ZeroedTips > 0) rows.Add("Zeroed tips: " + s.ZeroedTips);
            return rows.ToArray();
        }

        public static string TomorrowText(NightSummary s)
        {
            if (s.IsFinalNight || string.IsNullOrEmpty(s.NextNightNote)) return string.Empty;
            return TomorrowPrefix + s.NextNightNote;
        }

        public static string FinalHeadline(NightSummary s)
        {
            return s.PaidOff ? PaidOffHeadline : "Debt left: " + Money(s.DebtLeft) + ": you did not pay it off";
        }

        public static string Money(int amount) => "$" + amount;

        /// <summary>"+$6", "-$4" or "$0".</summary>
        public static string Signed(int amount)
        {
            if (amount == 0) return "$0";
            return amount > 0 ? "+$" + amount : "-$" + (-amount);
        }

        private static string SpeedLine(int adjust) =>
            (adjust < 0 ? "Speed penalty: " : "Speed bonus: ") + Signed(adjust);
    }
}
