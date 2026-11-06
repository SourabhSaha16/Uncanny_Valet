using System;

namespace UncannyValet.Core.Ids
{
    /// <summary>Stable id for one patron ticket during a night. Value 0 means "no ticket".</summary>
    public readonly struct TicketId : IEquatable<TicketId>
    {
        public readonly int Value;

        public TicketId(int value) => Value = value;

        public static TicketId None => default;
        public bool IsValid => Value != 0;

        public bool Equals(TicketId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is TicketId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Ticket#{Value}";

        public static bool operator ==(TicketId a, TicketId b) => a.Equals(b);
        public static bool operator !=(TicketId a, TicketId b) => !a.Equals(b);
    }
}
