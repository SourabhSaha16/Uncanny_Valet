using System;
using UnityEngine.InputSystem;

namespace UncannyValet.Powers
{
    /// <summary>Keys 1 to 4 on the keyboard, one per power slot. Keyboard only. Ignored while the blocker says so (typing in a panel, title screen).</summary>
    public sealed class KeyboardPowerInput : IPowerInput
    {
        private readonly Func<bool> _blocked;

        /// <param name="blocked">True while the keys must be ignored; null never blocks.</param>
        public KeyboardPowerInput(Func<bool> blocked = null) { _blocked = blocked; }

        public bool PowerPressed(int slot)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || slot < 0 || slot >= PowerKeyMap.SlotCount || (_blocked != null && _blocked())) return false;
            return keyboard[Key.Digit1 + slot].wasPressedThisFrame;
        }
    }
}
