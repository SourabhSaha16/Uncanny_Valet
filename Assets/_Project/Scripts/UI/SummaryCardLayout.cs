namespace UncannyValet.UI
{
    /// <summary>Pure size maths for the summary card, so the IMGUI code only draws.</summary>
    public static class SummaryCardLayout
    {
        /// <summary>Card height for a title, an optional headline, N text lines and the button.</summary>
        public static float Height(int lineCount, bool hasHeadline, float lineHeight, float padding, float buttonHeight)
        {
            var rows = 1 + (hasHeadline ? 1 : 0) + (lineCount < 0 ? 0 : lineCount);
            return padding * 3f + rows * lineHeight + buttonHeight;
        }

        /// <summary>Top-left corner of a box of the given size centred in the screen.</summary>
        public static void Centre(float screenW, float screenH, float w, float h, out float x, out float y)
        {
            x = (screenW - w) * 0.5f;
            y = (screenH - h) * 0.5f;
        }
    }
}
