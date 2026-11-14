using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UncannyValet.Powers;
using UncannyValet.Run;

namespace UncannyValet.Bootstrap
{
    /// <summary>The lot and power facts of the story cards, read from the run's lot definitions and the power definitions.</summary>
    public sealed class RunStoryFacts : IStoryFacts
    {
        private readonly RunDefinition _run;
        private readonly IReadOnlyList<PowerDefinition> _powers;

        public RunStoryFacts(RunDefinition run, IReadOnlyList<PowerDefinition> powers)
        {
            _run = run;
            _powers = powers;
        }

        public bool TryGetLot(LotId id, out StoryLotFacts facts)
        {
            facts = default;
            if (!_run.TryGet(id, out var lot)) return false;
            facts = new StoryLotFacts(id, lot.DisplayName, lot.Watcher != null ? lot.Watcher.SpeakerName : string.Empty, lot.BlessingThreshold);
            return true;
        }

        public bool TryGetPower(StoryPower power, out StoryPowerFacts facts)
        {
            facts = default;
            if (power == StoryPower.None) return false;
            for (var i = 0; i < _powers.Count; i++)
            {
                if (_powers[i] == null || _powers[i].Power != power) continue;
                facts = new StoryPowerFacts(power, _powers[i].DisplayName, _powers[i].Description);
                return true;
            }
            facts = new StoryPowerFacts(power, PowerDefaults.DisplayName(power), PowerDefaults.Description(power));
            return true;
        }
    }
}
