using UncannyValet.Core.Ids;

namespace UncannyValet.Core.Events
{
    /// <summary>
    /// A car or the valet bumped a pedestrian (published by Pedestrians). WorldX / WorldY locate the bump;
    /// Strength01 is the impact from 0 (a nudge) to 1 (a hard hit).
    /// </summary>
    public readonly struct PedestrianBumped : IGameEvent
    {
        public readonly float WorldX;
        public readonly float WorldY;
        public readonly float Strength01;
        public PedestrianBumped(float worldX, float worldY, float strength01) { WorldX = worldX; WorldY = worldY; Strength01 = strength01; }
    }
}
