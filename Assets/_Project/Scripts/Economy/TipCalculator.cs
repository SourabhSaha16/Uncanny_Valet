using System;
using UncannyValet.Core.Contracts;

namespace UncannyValet.Economy
{
    /// <summary>Base, plus a speed bonus inside the window, minus a penalty per minute after it. Never below 0.</summary>
    public sealed class TipCalculator : ITipCalculator
    {
        private readonly TipConfig _config;

        public TipCalculator(TipConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public int Calculate(in TipInput input)
        {
            var window = _config.SpeedBonusWindowMinutes;
            var tip = (float)input.BaseTip;
            if (input.WaitMinutes <= window)
            {
                tip += input.Impatient ? _config.ImpatientSpeedBonus : _config.SpeedBonus;
            }
            else
            {
                tip -= (input.WaitMinutes - window) * _config.PenaltyPerMinute;
            }
            return Math.Max(0, (int)Math.Round(tip, MidpointRounding.AwayFromZero));
        }
    }
}
