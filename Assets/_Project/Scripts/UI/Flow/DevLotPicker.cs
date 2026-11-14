using System.Collections.Generic;
using UncannyValet.Core.Contracts;

namespace UncannyValet.UI.Flow
{
    /// <summary>The developer lot picker as the flow sees it: choosing a lot goes through the flow (its intro card, then the shift) instead of loading the scene on its own.</summary>
    public sealed class DevLotPicker : ILotPicker
    {
        private readonly ILotPicker _lots;
        private readonly FlowController _flow;

        public DevLotPicker(ILotPicker lots, FlowController flow)
        {
            _lots = lots;
            _flow = flow;
        }

        public IReadOnlyList<LotChoice> Choices => _lots.Choices;

        public bool TryChoose(int index)
        {
            if (index < 0 || index >= _lots.Choices.Count) return false;
            _flow.DevStart(_lots.Choices[index].Id);
            return true;
        }
    }
}
