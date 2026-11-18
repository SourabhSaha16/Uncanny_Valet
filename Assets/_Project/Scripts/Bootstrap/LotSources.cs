using UncannyValet.Lot;
using UncannyValet.Lot.Handmade;

namespace UncannyValet.Bootstrap
{
    /// <summary>Where a scene's lot components become an <see cref="ILotSource"/>: the one place that knows which kinds of lot exist.</summary>
    public static class LotSources
    {
        /// <summary>The generated lot of the scene; null when there is no builder.</summary>
        public static ILotSource Parametric(LotLayoutBuilder lot, LotDecor decor)
            => lot != null ? new ParametricLotSource(lot, decor) : null;

        /// <summary>The hand-made lot of the scene; null when there is none.</summary>
        public static ILotSource Handmade(HandmadeLot lot) => lot != null ? new HandmadeLotSource(lot) : null;

        /// <summary>The hand-made lot when the scene has one, otherwise the generated lot.</summary>
        public static ILotSource Of(HandmadeLot handmade, LotLayoutBuilder lot, LotDecor decor) => Handmade(handmade) ?? Parametric(lot, decor);
    }
}
