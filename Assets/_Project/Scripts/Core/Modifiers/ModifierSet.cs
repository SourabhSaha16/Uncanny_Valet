using System;

namespace UncannyValet.Core.Modifiers
{
    /// <summary>An immutable set of <see cref="ModifierId"/>s (a car's modifier list as plain data). Allocates nothing.</summary>
    public readonly struct ModifierSet : IEquatable<ModifierSet>
    {
        /// <summary>Most ids a set can hold, counting <see cref="ModifierId.None"/> (which is never stored).</summary>
        public const int Capacity = 32;

        private readonly uint _bits;

        private ModifierSet(uint bits) { _bits = bits; }

        /// <summary>The empty set.</summary>
        public static ModifierSet None => default;

        /// <summary>A set of exactly one modifier.</summary>
        public static ModifierSet Of(ModifierId id) => None.With(id);

        /// <summary>A set of exactly two modifiers.</summary>
        public static ModifierSet Of(ModifierId first, ModifierId second) => None.With(first).With(second);

        public bool IsEmpty => _bits == 0u;

        /// <summary>How many modifiers the set holds.</summary>
        public int Count
        {
            get
            {
                var n = 0;
                for (var b = _bits; b != 0u; b &= b - 1u) n++;
                return n;
            }
        }

        public bool Contains(ModifierId id) => id != ModifierId.None && (_bits & Bit(id)) != 0u;

        /// <summary>The set plus <paramref name="id"/>; <see cref="ModifierId.None"/> adds nothing.</summary>
        public ModifierSet With(ModifierId id) => id == ModifierId.None ? this : new ModifierSet(_bits | Bit(id));

        /// <summary>The set minus <paramref name="id"/>.</summary>
        public ModifierSet Without(ModifierId id) => id == ModifierId.None ? this : new ModifierSet(_bits & ~Bit(id));

        /// <summary>This set minus every id of <paramref name="other"/> (for example the open ones: held without done).</summary>
        public ModifierSet Without(ModifierSet other) => new ModifierSet(_bits & ~other._bits);

        /// <summary>The ids in both sets.</summary>
        public ModifierSet Intersect(ModifierSet other) => new ModifierSet(_bits & other._bits);

        /// <summary>The id at <paramref name="index"/> (0 to <see cref="Count"/> - 1), in id order; <see cref="ModifierId.None"/> past the end.</summary>
        public ModifierId At(int index)
        {
            var seen = 0;
            for (var i = 1; i < Capacity; i++)
            {
                if ((_bits & (1u << i)) == 0u) continue;
                if (seen == index) return (ModifierId)i;
                seen++;
            }
            return ModifierId.None;
        }

        public bool Equals(ModifierSet other) => _bits == other._bits;
        public override bool Equals(object obj) => obj is ModifierSet other && Equals(other);
        public override int GetHashCode() => (int)_bits;
        public static bool operator ==(ModifierSet a, ModifierSet b) => a._bits == b._bits;
        public static bool operator !=(ModifierSet a, ModifierSet b) => a._bits != b._bits;

        private static uint Bit(ModifierId id)
        {
            var n = (int)id;
            if (n < 1 || n >= Capacity) throw new ArgumentOutOfRangeException(nameof(id), id, "Modifier id does not fit a ModifierSet.");
            return 1u << n;
        }
    }
}
