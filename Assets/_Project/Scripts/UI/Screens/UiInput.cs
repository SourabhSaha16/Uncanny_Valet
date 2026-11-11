using UnityEngine.InputSystem;

namespace UncannyValet.UI.Screens
{
    /// <summary>The key presses the title, how-to-play and results screens react to.</summary>
    public static class UiInput
    {
        /// <summary>True the frame any key or the left mouse button went down.</summary>
        public static bool AnyPressed()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            return keyboard != null && keyboard.anyKey.wasPressedThisFrame
                || mouse != null && mouse.leftButton.wasPressedThisFrame;
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
