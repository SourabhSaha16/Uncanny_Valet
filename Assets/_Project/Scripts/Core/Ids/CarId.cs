using System;

namespace UncannyValet.Core.Ids
{
    /// <summary>Stable id for one car during a night. Value 0 means "no car".</summary>
    public readonly struct CarId : IEquatable<CarId>
    {
        public readonly int Value;

        public CarId(int value) => Value = value;

        public static CarId None => default;
        public bool IsValid => Value != 0;

        public bool Equals(CarId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is CarId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Car#{Value}";

        public static bool operator ==(CarId a, CarId b) => a.Equals(b);
        public static bool operator !=(CarId a, CarId b) => !a.Equals(b);
    }
}
