using System.Collections.Generic;

namespace UncannyValet.Cars
{
    /// <summary>An ordered list of <see cref="IDriveModifier"/>s applied to the input every physics step without allocating.</summary>
    public sealed class DriveModifierStack
    {
        private readonly List<IDriveModifier> _modifiers = new List<IDriveModifier>();

        /// <summary>Number of modifiers in the stack.</summary>
        public int Count => _modifiers.Count;

        /// <summary>Adds a modifier after every modifier of an equal or lower order.</summary>
        public void Add(IDriveModifier modifier)
        {
            var index = _modifiers.Count;
            while (index > 0 && _modifiers[index - 1].Order > modifier.Order) index--;
            _modifiers.Insert(index, modifier);
        }

        /// <summary>Removes a modifier; false when it was not in the stack.</summary>
        public bool Remove(IDriveModifier modifier) => _modifiers.Remove(modifier);

        /// <summary>Runs the input through every modifier in order.</summary>
        public DriveInput Apply(DriveInput input)
        {
            for (var i = 0; i < _modifiers.Count; i++) input = _modifiers[i].Modify(input);
            return input;
        }
    }
}
