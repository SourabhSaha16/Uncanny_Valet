using UncannyValet.Core.Events;

namespace UncannyValet.Powers
{
    /// <summary>Level 3: instant. The Lot and Patrons modules do the work when they hear PowerActivated(ParkSmash); nothing to hold here.</summary>
    public sealed class ParkSmashEffect : IPowerEffect
    {
        public PowerKind Kind => PowerKind.ParkSmash;
        public bool IsActive => false;
        public void Activate() { }
        public void Tick(float deltaSeconds) { }
        public void End() { }
    }
}
