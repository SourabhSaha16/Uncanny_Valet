using System.Collections.Generic;

namespace UncannyValet.Core.Contracts
{
    /// <summary>The temporary developer path to start any lot from the title screen (superseded by the story flow).</summary>
    public interface ILotPicker
    {
        /// <summary>The lots of the run, in play order.</summary>
        IReadOnlyList<LotChoice> Choices { get; }

        /// <summary>Loads the lot's scene. False (and nothing happens) when the index is out of range.</summary>
        bool TryChoose(int index);
    }
}
