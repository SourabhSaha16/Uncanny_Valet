using UncannyValet.Core.Config;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Director;
using UncannyValet.Economy;
using UncannyValet.Patrons;

namespace UncannyValet.Bootstrap
{
    /// <summary>Copies every tuning number from the config assets into the store, once, at startup.</summary>
    public static class TuningSeeder
    {
        /// <param name="spotCount">Spots in the lot; used when the plan says 0 ("all spots open").</param>
        /// <param name="director">The Patron Director's numbers; null skips them (its keys then stay unseeded).</param>
        /// <param name="profile">The lot's director profile, which supplies the arrival budget.</param>
        public static void Seed(ITuning t, ShiftConfig shift, PatronConfig patrons, TipConfig tips, int spotCount,
            DirectorConfig director = null, DirectorProfile profile = null)
        {
            if (director != null && profile != null) director.SeedInto(t, profile);

            t.Seed(TuningKey.GameMinutesPerSecond, shift.GameMinutesPerSecond);
            t.Seed(TuningKey.ShiftLengthSeconds, shift.ShiftLengthSeconds);

            var service = patrons.Tuning;
            t.Seed(TuningKey.PickupWindowMinutes, patrons.PickupWindowMinutes);
            t.Seed(TuningKey.StayMinMinutes, patrons.StayRange.x);
            t.Seed(TuningKey.StayMaxMinutes, patrons.StayRange.y);
            t.Seed(TuningKey.ArrivalGapMinMinutes, patrons.ArrivalIntervalRange.x);
            t.Seed(TuningKey.ArrivalGapMaxMinutes, patrons.ArrivalIntervalRange.y);
            t.Seed(TuningKey.ImpatientChance, patrons.ImpatientChance);
            t.Seed(TuningKey.ChargeMinutes, service.ChargeMinutes);
            t.Seed(TuningKey.WashMinutes, service.WashMinutes);

            t.Seed(TuningKey.BaseTip, patrons.BaseTip);
            t.Seed(TuningKey.ChargeBonus, tips.ChargeBonus);
            t.Seed(TuningKey.CleanBonus, tips.CleanBonus);
            t.Seed(TuningKey.SpeedWindowMinutes, tips.SpeedBonusWindowMinutes);

            var plan = shift.Plan;
            t.Seed(TuningKey.PlanMaxWaitingCars, plan.MaxWaitingCars);
            t.Seed(TuningKey.PlanArrivalGapScale, plan.ArrivalGapScale);
            t.Seed(TuningKey.PlanActiveSpots, plan.ActiveSpots > 0 ? plan.ActiveSpots : spotCount);
            t.Seed(TuningKey.PlanEvChance, plan.EvChance);
            t.Seed(TuningKey.PlanDirtyChance, plan.DirtyChance);
            t.Seed(TuningKey.PlanLateEvChance, plan.LateEvChance);
            t.Seed(TuningKey.PlanLateEvStart, plan.LateEvStart);
        }
    }
}
