using System;

namespace UncannyValet.Core.Ids
{
    /// <summary>Identifies one pedestrian body during a run (the Unity instance id of its object). Value 0 means "nobody".</summary>
    public readonly struct PedestrianId : IEquatable<PedestrianId>
    {
        public readonly int Value;

        public PedestrianId(int value) => Value = value;

        public bool IsValid => Value != 0;

        public bool Equals(PedestrianId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is PedestrianId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Pedestrian#{Value}";

        public static bool operator ==(PedestrianId a, PedestrianId b) => a.Equals(b);
        public static bool operator !=(PedestrianId a, PedestrianId b) => !a.Equals(b);
    }
}
