using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows for which cars the director sends and the squeeze moments (dilemmas) it sets up.</summary>
    public static class DirectorMixRows
    {
        private const string Mix = "Car mix";
        private const string Dilemmas = "Dilemmas";

        public static readonly TuningInfo[] MixRows =
        {
            new TuningInfo(TuningKey.DirWeightPlain, Mix, "Base weight: plain cars", 0, 100, 0.5f,
                "Relative chance of a car needing neither wash nor charge."),
            new TuningInfo(TuningKey.DirWeightDirty, Mix, "Base weight: dirty cars", 0, 100, 0.5f),
            new TuningInfo(TuningKey.DirWeightEv, Mix, "Base weight: EVs", 0, 100, 0.5f),
            new TuningInfo(TuningKey.DirWeightDirtyEv, Mix, "Base weight: dirty EVs", 0, 100, 0.5f),
            new TuningInfo(TuningKey.DirLoadUnderHeavy, Mix, "Dirty-car weight when you are under-worked (x)", 0.1f, 4f, 0.1f),
            new TuningInfo(TuningKey.DirLoadOverHeavy, Mix, "Dirty-car weight when you are overwhelmed (x)", 0.1f, 4f, 0.1f),
            new TuningInfo(TuningKey.DirLoadOverPlain, Mix, "Plain-car weight when you are overwhelmed (x)", 0.1f, 4f, 0.1f),
            new TuningInfo(TuningKey.DirStreakLimit, Mix, "Most dirty or EV cars in a row", 1, 6, 1),
            new TuningInfo(TuningKey.DirMixTolerance, Mix, "How far the shift's car mix may drift (0.1 = 10 points)", 0.02f, 0.5f, 0.01f),
        };

        public static readonly TuningInfo[] DilemmaRows =
        {
            new TuningInfo(TuningKey.DirDilemmaWashBlocked, Dilemmas, "Dirty-car boost when the wash is blocked (x)", 1, 6, 0.1f),
            new TuningInfo(TuningKey.DirDilemmaWashBusy, Dilemmas, "Dirty-car boost when the wash is busy (x)", 1, 6, 0.1f),
            new TuningInfo(TuningKey.DirDilemmaBaysBlocked, Dilemmas, "EV boost when every bay is blocked (x)", 1, 6, 0.1f),
            new TuningInfo(TuningKey.DirDilemmaBaySquatted, Dilemmas, "EV boost when a plain car squats a bay (x)", 1, 6, 0.1f),
            new TuningInfo(TuningKey.DirDilemmaLastBay, Dilemmas, "EV boost when only one bay is left (x)", 1, 6, 0.1f),
            new TuningInfo(TuningKey.DirDilemmaLotTight, Dilemmas, "Plain-car boost when the lot is nearly full (x)", 1, 6, 0.1f),
            new TuningInfo(TuningKey.DirDilemmaChain, Dilemmas, "Dirty-EV boost when wash and bays are all stuck (x)", 1, 6, 0.1f),
            new TuningInfo(TuningKey.DirLotTightSpots, Dilemmas, "Lot counts as nearly full at this many free spots or fewer", 0, 9, 1),
            new TuningInfo(TuningKey.DirSolvablePickupSeconds, Dilemmas, "A full-lot dilemma is fair if a pickup is due within (s)", 0, 60, 1),
            new TuningInfo(TuningKey.DirDilemmaCooldown, Dilemmas, "Pause after a dilemma before the next one (s)", 0, 120, 1),
            new TuningInfo(TuningKey.DirDilemmaBlackout, Dilemmas, "No dilemmas for this long after an angry patron (s)", 0, 120, 1),
            new TuningInfo(TuningKey.DirHeadroomGap, Dilemmas, "Dilemmas need you at most this far over target", 0f, 1f, 0.01f),
        };
    }
}
