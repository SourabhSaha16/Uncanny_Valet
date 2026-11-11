using UncannyValet.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UncannyValet.Valet
{
    /// <summary>WASD/arrows to move, E to interact. Polled each frame via the Input System package.</summary>
    public sealed class KeyboardInputSource : MonoBehaviour, IInputSource
    {
        public Vector2 Move { get; private set; }

        public bool InteractPressed { get; private set; }

        /// <summary>Left Shift or the gamepad right shoulder: the one Dash/Handbrake button (handbrake in a car, dash on foot).</summary>
        public bool Drift { get; private set; }

        private void Update()
        {
            var pad = Gamepad.current;
            Drift = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) || (pad != null && pad.rightShoulder.isPressed);
            var kb = Keyboard.current;
            if (kb == null) { Move = Vector2.zero; InteractPressed = false; return; }

            var x = Axis(kb.dKey.isPressed || kb.rightArrowKey.isPressed, kb.aKey.isPressed || kb.leftArrowKey.isPressed);
            var y = Axis(kb.wKey.isPressed || kb.upArrowKey.isPressed, kb.sKey.isPressed || kb.downArrowKey.isPressed);
            Move = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            InteractPressed = kb.eKey.wasPressedThisFrame;
        }

        private static float Axis(bool positive, bool negative) => (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }
}
