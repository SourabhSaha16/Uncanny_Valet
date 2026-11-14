using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Loads a lot by its id through the picker.</summary>
    public static class LotPickerExtensions
    {
        /// <summary>Loads the scene of the lot; false (and nothing happens) when the picker does not list it.</summary>
        public static bool TryChoose(this ILotPicker picker, LotId lot)
        {
            for (var i = 0; i < picker.Choices.Count; i++)
                if (picker.Choices[i].Id == lot) return picker.TryChoose(i);
            return false;
        }
    }
}
