using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of the screen shake on a hard hit (Bootstrap owns the values), in designer language.</summary>
    public static class ShakeRows
    {
        public const string Group = "Screen shake";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.ShakeStrength, Group, "Screen shake: strength (units)", 0f, 1.5f, 0.05f,
                "How far the view jolts at the biggest hit, in world units. 0 turns the shake off. Only hits that damage the car you are driving shake the screen."),
            new TuningInfo(TuningKey.ShakeSeconds, Group, "Screen shake: length (s)", 0.05f, 1.5f, 0.05f,
                "How long the shake lasts before it dies away."),
            new TuningInfo(TuningKey.ShakeFullForce, Group, "Screen shake: biggest hit it scales to (impact force)", 1f, 20f, 0.5f,
                "A hit just above the damage limit shakes a little; a hit this hard shakes at full strength. Harder hits do not shake more."),
        };
    }
}
