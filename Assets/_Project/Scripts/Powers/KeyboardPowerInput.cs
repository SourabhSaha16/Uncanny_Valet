using UnityEngine.InputSystem;

namespace UncannyValet.Powers
{
    /// <summary>Q on the keyboard or the left shoulder button on a gamepad.</summary>
    public sealed class KeyboardPowerInput : IPowerInput
    {
        public bool ActivatePressed
        {
            get
            {
                var keyboard = Keyboard.current;
                var pad = Gamepad.current;
                return (keyboard != null && keyboard.qKey.wasPressedThisFrame)
                    || (pad != null && pad.leftShoulder.wasPressedThisFrame);
            }
        }
    }
}
