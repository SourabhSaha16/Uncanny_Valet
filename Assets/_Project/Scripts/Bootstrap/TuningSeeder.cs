using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Economy;
using UncannyValet.Patrons;

namespace UncannyValet.Bootstrap
{
    /// <summary>Copies every tuning number from the config assets into the store, once, at startup.</summary>
    public static class TuningSeeder
    {
        /// <param name="spotCount">Spots in the lot; used when the plan says 0 ("all spots open").</param>
        public static void Seed(ITuning t, RunConfig run, PatronConfig patrons, TipConfig tips,
            NightPlan firstNight, int spotCount)
        {
            t.Seed(TuningKey.GameMinutesPerSecond, run.GameMinutesPerSecond);
            t.Seed(TuningKey.NightLengthMinutes, run.NightLengthMinutes);

            var service = patrons.Tuning;
            t.Seed(TuningKey.ParkWindowMinutes, patrons.ParkWindowMinutes);
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

            t.Seed(TuningKey.PlanMaxWaitingCars, firstNight.MaxWaitingCars);
            t.Seed(TuningKey.PlanArrivalGapScale, firstNight.ArrivalGapScale);
            t.Seed(TuningKey.PlanActiveSpots, firstNight.ActiveSpots > 0 ? firstNight.ActiveSpots : spotCount);
            t.Seed(TuningKey.PlanEvChance, firstNight.EvChance);
            t.Seed(TuningKey.PlanDirtyChance, firstNight.DirtyChance);
            t.Seed(TuningKey.PlanLateEvChance, firstNight.LateEvChance);
            t.Seed(TuningKey.PlanLateEvStart, firstNight.LateEvStart);
        }
    }
}
