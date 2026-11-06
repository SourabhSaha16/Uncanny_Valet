using System;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Wraps the real input and reports no input while a text field (the tuning panel) has keyboard focus.</summary>
    public sealed class GatedInputSource : IInputSource
    {
        private readonly IInputSource _inner;
        private readonly Func<bool> _blocked;

        public GatedInputSource(IInputSource inner, Func<bool> blocked)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _blocked = blocked ?? throw new ArgumentNullException(nameof(blocked));
        }

        public Vector2 Move => _blocked() ? Vector2.zero : _inner.Move;

        public bool InteractPressed => !_blocked() && _inner.InteractPressed;
    }
}
