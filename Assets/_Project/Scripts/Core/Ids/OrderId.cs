using System;

namespace UncannyValet.Core.Ids
{
    /// <summary>Stable id for one boss order during a run. Value 0 means "no order".</summary>
    public readonly struct OrderId : IEquatable<OrderId>
    {
        public readonly int Value;

        public OrderId(int value) => Value = value;

        public static OrderId None => default;
        public bool IsValid => Value != 0;

        public bool Equals(OrderId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is OrderId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Order#{Value}";

        public static bool operator ==(OrderId a, OrderId b) => a.Equals(b);
        public static bool operator !=(OrderId a, OrderId b) => !a.Equals(b);
    }
}
