using UnityEngine.InputSystem;

namespace UncannyValet.UI.Screens
{
    /// <summary>The key presses the title, how-to-play and results screens react to.</summary>
    public static class UiInput
    {
        private const int DigitCount = 10;

        /// <summary>True the frame any key or the left mouse button went down.</summary>
        public static bool AnyPressed()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return keyboard != null && keyboard.anyKey.wasPressedThisFrame
                || mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        /// <summary>True the frame a digit key went down; index is 0 for key 1 up to 8 for key 9, and 9 for key 0.</summary>
        public static bool TryDigitPressed(out int index)
        {
            index = -1;
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            for (var i = 0; i < DigitCount; i++)
            {
                var key = i < DigitCount - 1 ? Key.Digit1 + i : Key.Digit0;
                if (!keyboard[key].wasPressedThisFrame) continue;
                index = i;
                return true;
            }
            return false;
        }

        /// <summary>True the frame Enter or Space went down (retry on the results screen).</summary>
        public static bool RetryPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.enterKey.wasPressedThisFrame
                || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame);
        }
    }
}
