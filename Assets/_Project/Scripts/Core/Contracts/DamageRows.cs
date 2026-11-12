using UncannyValet.Core.Events;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Advanced panel rows of car damage (Cars owns the values), in designer language.</summary>
    public static class DamageRows
    {
        public const string Group = "Car damage";

        public static readonly TuningInfo[] Rows =
        {
            new TuningInfo(TuningKey.DamageMinForce, Group, "Damage: smallest hit that counts (impact force)", 0.2f, 8f, 0.1f,
                "A hit must change the car's speed by at least this much (units per second) to damage it; gentle taps below it are free. A full-speed wall hit is about 8."),
            new TuningInfo(TuningKey.DamagePercentPerForce, Group, "Damage per unit of impact force (%)", 0f, 50f, 0.5f,
                "Percent of damage added for each unit of force above the smallest hit. 8 means a hit 3 above the limit adds 24%; 100% total wrecks the car and its tip is $0."),
        };
    }
}
