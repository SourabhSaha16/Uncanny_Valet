using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>
    /// Pure, deterministic choices for the crowd: who wears what, who walks which route, where along it they start
    /// and how fast. The same index always gives the same answer.
    /// </summary>
    public static class PedestrianPlan
    {
        private const int SkinStride = 1;
        private const int HairStride = 3;
        private const int OutfitStride = 5;
        private const int OutfitOffset = 2;

        /// <summary>The look of pedestrian <paramref name="index"/>; the first ones differ in every layer where counts allow.</summary>
        public static PedestrianLook Look(int index, int skins, int hairs, int outfits)
            => new PedestrianLook(Pick(index, skins, SkinStride, 0), Pick(index, hairs, HairStride, 0),
                Pick(index, outfits, OutfitStride, OutfitOffset));

        /// <summary>
        /// Index into a list of <paramref name="count"/> items: <c>(index * stride + offset) mod count</c> with the stride
        /// nudged up until it shares no factor with the count, so consecutive indices never repeat before the list does.
        /// </summary>
        public static int Pick(int index, int count, int preferredStride, int offset)
        {
            if (count <= 0) return 0;
            var stride = CoprimeStride(count, preferredStride);
            return (int)(((long)index * stride + offset) % count);
        }

        /// <summary>The smallest stride at or above <paramref name="preferred"/> (at least 1) that is coprime with <paramref name="count"/>.</summary>
        public static int CoprimeStride(int count, int preferred)
        {
            var stride = Mathf.Max(1, preferred);
            while (count > 1 && Gcd(stride, count) != 1) stride++;
            return stride;
        }

        /// <summary>The route a pedestrian walks; pedestrians are dealt out to the routes in turn. -1 when there are none.</summary>
        public static int RouteFor(int index, int routeCount) => routeCount <= 0 ? -1 : index % routeCount;

        /// <summary>
        /// Where in its walk (0..1 of the there-and-back cycle) a pedestrian starts, spreading the walkers that share a
        /// route evenly and giving each route a different phase.
        /// </summary>
        public static float StartFraction(int index, int routeCount, int total)
        {
            if (routeCount <= 0 || total <= 0) return 0f;
            var route = index % routeCount;
            var onRoute = (total - route + routeCount - 1) / routeCount;
            var slot = index / routeCount;
            var fraction = (slot + 0.5f) / Mathf.Max(1, onRoute) + route * 0.31f;
            return fraction - Mathf.Floor(fraction);
        }

        /// <summary>Walking speed between <paramref name="min"/> and <paramref name="max"/>, varied by index.</summary>
        public static float Speed(int index, float min, float max) => Mathf.Lerp(min, max, Hash01(index));

        /// <summary>A repeatable number in 0..1 for an index.</summary>
        public static float Hash01(int index)
        {
            var h = unchecked((uint)index * 2654435761u);
            h ^= h >> 15;
            return (h & 0xFFFFu) / 65535f;
        }

        private static int Gcd(int a, int b)
        {
            while (b != 0)
            {
                var t = a % b;
                a = b;
                b = t;
            }
            return a;
        }
    }
}
