using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows for how the director reacts to the player: mercy when overwhelmed, reward for good play.</summary>
    public static class DirectorPlayerRows
    {
        private const string Mercy = "Mercy";
        private const string Skill = "Skill reward";

        public static readonly TuningInfo[] MercyRows =
        {
            new TuningInfo(TuningKey.DirReliefWorkload, Mercy, "Back off when workload is above", 0.5f, 3f, 0.05f),
            new TuningInfo(TuningKey.DirReliefPressure, Mercy, "Back off when a patron's patience is nearly gone (0-1)", 0.3f, 1f, 0.01f),
            new TuningInfo(TuningKey.DirReliefAngryCount, Mercy, "Back off after this many angry patrons", 1, 6, 1),
            new TuningInfo(TuningKey.DirReliefAngryWindow, Mercy, "Angry patrons are counted over (s)", 5, 120, 1),
            new TuningInfo(TuningKey.DirReliefHold, Mercy, "Stop all arrivals for (s)", 1, 40, 1),
            new TuningInfo(TuningKey.DirReliefPlainCars, Mercy, "Then send this many plain cars", 0, 6, 1),
            new TuningInfo(TuningKey.DirReliefCooldown, Mercy, "Pause before mercy can trigger again (s)", 0, 120, 1),
            new TuningInfo(TuningKey.DirAssist, Mercy, "Assist mode (good play never raises difficulty)", 0, 1, 1,
                "Clamps the skill reward at zero and softens every dilemma."),
            new TuningInfo(TuningKey.DirAssistDilemmaScale, Mercy, "Assist mode: dilemma strength kept", 0f, 1f, 0.05f),
        };

        public static readonly TuningInfo[] SkillRows =
        {
            new TuningInfo(TuningKey.DirFormInfluence, Skill, "How much good play raises the target workload", 0f, 0.5f, 0.01f),
            new TuningInfo(TuningKey.DirFormUpdate, Skill, "How often your form is re-judged (s)", 1, 60, 1),
            new TuningInfo(TuningKey.DirFormMaxStep, Skill, "Biggest form change per re-judging", 0.05f, 1f, 0.05f),
            new TuningInfo(TuningKey.DirFormWindow, Skill, "How far back blessings and angry patrons count (s)", 10, 180, 1),
            new TuningInfo(TuningKey.DirFormWarmup, Skill, "Grace period at shift start (s)", 0, 120, 1),
            new TuningInfo(TuningKey.DirFormBlessingWeight, Skill, "Weight of blessing pace", 0f, 1.5f, 0.05f),
            new TuningInfo(TuningKey.DirFormAngryWeight, Skill, "Weight of angry patrons (lowers form)", 0f, 1.5f, 0.05f),
            new TuningInfo(TuningKey.DirFormFavourWeight, Skill, "Weight of the favour", 0f, 1.5f, 0.05f),
            new TuningInfo(TuningKey.DirFormAngryFull, Skill, "Angry patrons that give the full penalty", 1, 6, 1),
        };
    }
}
