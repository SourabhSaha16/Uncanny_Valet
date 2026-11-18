using UncannyValet.Lot;

namespace UncannyValet.Bootstrap
{
    /// <summary>
    /// Where a scene's lot components become an <see cref="ILotSource"/>. This is the extension point for new kinds of lot: a hand-made lot (#194)
    /// adds its own factory method here and its own scene field, and nothing else in the bootstrap changes.
    /// </summary>
    public static class LotSources
    {
        /// <summary>The generated lot of the scene; null when there is no builder.</summary>
        public static ILotSource Parametric(LotLayoutBuilder lot, LotDecor decor)
            => lot != null ? new ParametricLotSource(lot, decor) : null;
    }
}
