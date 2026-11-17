using System.Collections.Generic;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Run
{
    /// <summary>
    /// The lots of the run in play order (Midgard training, Midgard crazy, Vanaheim training, ... Helheim crazy,
    /// Asgard). Each world task appends its lots. Read-only at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Run Definition", fileName = "RunDefinition")]
    public sealed class RunDefinition : ScriptableObject
    {
        [SerializeField, Tooltip("Every lot of the run, in play order.")]
        private List<LotDefinition> _lots = new List<LotDefinition>();

        public IReadOnlyList<LotDefinition> Lots => _lots;

        /// <summary>The position of the lot in play order, or -1 when the run does not list it.</summary>
        public int IndexOf(LotId id)
        {
            for (var i = 0; i < _lots.Count; i++)
                if (_lots[i] != null && _lots[i].Id == id) return i;
            return -1;
        }

        /// <summary>Finds the lot's definition.</summary>
        public bool TryGet(LotId id, out LotDefinition lot)
        {
            var index = IndexOf(id);
            lot = index >= 0 ? _lots[index] : null;
            return lot != null;
        }

        /// <summary>The lot after <paramref name="id"/> in play order; false for the last lot or an unlisted one.</summary>
        public bool TryGetNext(LotId id, out LotDefinition next)
        {
            var index = IndexOf(id);
            next = index >= 0 && index + 1 < _lots.Count ? _lots[index + 1] : null;
            return next != null;
        }

        /// <summary>The first lot of the run; false when the run is empty.</summary>
        public bool TryGetFirst(out LotDefinition first)
        {
            first = _lots.Count > 0 ? _lots[0] : null;
            return first != null;
        }

        /// <summary>Checks that every lot is present and valid and that no LotId or scene repeats.</summary>
        public bool TryValidate(out string problem)
        {
            problem = null;
            for (var i = 0; i < _lots.Count && problem == null; i++)
            {
                if (!LotDefinitionRules.TryValidate(_lots[i], out problem)) break;
                problem = FindRepeat(i);
            }
            if (problem == null && _lots.Count == 0) problem = name + ": the run lists no lots";
            return problem == null;
        }

        private string FindRepeat(int i)
        {
            for (var j = 0; j < i; j++)
            {
                if (_lots[j].Id == _lots[i].Id) return name + ": " + _lots[i].Id + " is listed twice";
                if (_lots[j].SceneName == _lots[i].SceneName) return name + ": scene " + _lots[i].SceneName + " is used by two lots";
            }
            return null;
        }
    }
}
