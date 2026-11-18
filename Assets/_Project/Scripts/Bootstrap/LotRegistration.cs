using UncannyValet.Core;
using UncannyValet.Core.Contracts;

namespace UncannyValet.Bootstrap
{
    /// <summary>Registers what a lot provides in <see cref="Services"/>, in the two steps the start-up order needs.</summary>
    public static class LotRegistration
    {
        /// <summary>The layout and, when the lot has them, the bay rectangles. Safe before the lot is active.</summary>
        public static void RegisterLayout(ILotSource source)
        {
            Services.Register<ILotLayout>(source.Layout);
            if (source.Spots != null) Services.Register<ISpotGeometry>(source.Spots);
        }

        /// <summary>The road. Call after <see cref="ILotSource.Root"/> is active, which is when the lot builds it.</summary>
        public static void RegisterRoad(ILotSource source) => Services.Register<IRoadLayout>(source.Road);

        /// <summary>Layout, bays and road together, for a lot that is already active.</summary>
        public static void RegisterAll(ILotSource source)
        {
            RegisterLayout(source);
            RegisterRoad(source);
        }
    }
}
