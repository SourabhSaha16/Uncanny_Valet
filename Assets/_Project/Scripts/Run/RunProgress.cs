using System;
using System.Collections.Generic;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;

namespace UncannyValet.Run
{
    /// <summary>
    /// The run state: the current lot, the powers earned and their levels, the cleared lots (per LotId, so a world's
    /// training and crazy lots are separate) and the best result per lot. Plain C#, in memory for the browser session.
    /// </summary>
    public sealed class RunProgress : IRunProgress
    {
        private readonly RunDefinition _run;
        private readonly List<StoryPower> _earned = new List<StoryPower>();
        private readonly Dictionary<StoryPower, int> _levels = new Dictionary<StoryPower, int>();
        private readonly HashSet<LotId> _cleared = new HashSet<LotId>();
        private readonly Dictionary<LotId, int> _best = new Dictionary<LotId, int>();

        public RunProgress(RunDefinition run)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
        }

        public bool HasCurrentLot { get; private set; }
        public LotId CurrentLot { get; private set; }
        public IReadOnlyList<StoryPower> EarnedPowers => _earned;

        public int PowerLevel(StoryPower power) => _levels.TryGetValue(power, out var level) ? level : 0;

        public bool IsCleared(LotId lot) => _cleared.Contains(lot);

        public bool IsWorldCleared(World world) =>
            _cleared.Contains(new LotId(world, LotKind.Crazy)) || _cleared.Contains(new LotId(world, LotKind.Finale));

        public int BestBlessings(LotId lot) => _best.TryGetValue(lot, out var best) ? best : 0;

        /// <summary>Records the lot being played.</summary>
        public void SetCurrentLot(LotId lot)
        {
            CurrentLot = lot;
            HasCurrentLot = true;
        }

        /// <summary>Earns the power or raises its level; a lower level than owned changes nothing. No-op for None.</summary>
        public void GrantPower(StoryPower power, int level)
        {
            if (power == StoryPower.None || level <= PowerLevel(power)) return;
            if (!_levels.ContainsKey(power)) _earned.Add(power);
            _levels[power] = level;
        }

        /// <summary>Marks the lot cleared (kept for the rest of the run).</summary>
        public void MarkCleared(LotId lot) => _cleared.Add(lot);

        /// <summary>Keeps the highest blessing total seen for the lot.</summary>
        public void RecordBest(LotId lot, int blessings)
        {
            if (blessings > BestBlessings(lot)) _best[lot] = blessings;
        }

        /// <summary>The next lot in the run's play order after the current one (the first lot when none was started).</summary>
        public bool TryGetNextLot(out LotDefinition next) =>
            HasCurrentLot ? _run.TryGetNext(CurrentLot, out next) : _run.TryGetFirst(out next);

        /// <summary>Forgets everything: powers, upgrades, cleared lots and best results (a fresh run).</summary>
        public void StartNewRun()
        {
            HasCurrentLot = false;
            CurrentLot = default;
            _earned.Clear();
            _levels.Clear();
            _cleared.Clear();
            _best.Clear();
        }
    }
}
