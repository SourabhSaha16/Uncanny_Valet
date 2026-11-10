using System.Collections.Generic;
using UncannyValet.Core.Events;

namespace UncannyValet.Director
{
    /// <summary>The rule list: the one place a new dilemma rule is added.</summary>
    public static class DilemmaRules
    {
        /// <summary>All seven rules from the spec, sharing one live settings object.</summary>
        public static IDilemmaRule[] CreateAll(DilemmaRuleSettings settings) => new IDilemmaRule[]
        {
            new WashBlockedRule(settings), new WashBusyRule(settings), new BaysBlockedRule(settings),
            new BaySquattedRule(settings), new LastBayRule(settings), new LotTightRule(settings),
            new ChainRule(settings),
        };

        /// <summary>The rule of a kind, or null when the list has none.</summary>
        public static IDilemmaRule Find(IReadOnlyList<IDilemmaRule> rules, DilemmaKind kind)
        {
            for (var i = 0; i < rules.Count; i++) if (rules[i].Kind == kind) return rules[i];
            return null;
        }
    }
}
