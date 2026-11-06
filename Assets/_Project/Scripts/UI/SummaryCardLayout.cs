namespace UncannyValet.UI
{
    /// <summary>Pure size maths for the summary card, so the IMGUI code only draws.</summary>
    public static class SummaryCardLayout
    {
        /// <summary>Card height for a title, an optional headline, N text lines and the button.</summary>
        public static float Height(int lineCount, bool hasHeadline, float lineHeight, float padding, float buttonHeight)
        {
            return Height(lineCount, hasHeadline, 0, 0, lineHeight, padding, buttonHeight);
        }

        /// <summary>
        /// Height with a tip-calculation block (header + calcRows) and a "Tomorrow" block of noteRows wrapped
        /// lines (plus padding above and below it). Zero rows leave a block out.
        /// </summary>
        public static float Height(int lineCount, bool hasHeadline, int calcRows, int noteRows,
            float lineHeight, float padding, float buttonHeight)
        {
            var rows = 1 + (hasHeadline ? 1 : 0) + (lineCount < 0 ? 0 : lineCount);
            if (calcRows > 0) rows += 1 + calcRows;
            var height = padding * 3f + rows * lineHeight + buttonHeight;
            if (noteRows > 0) height += noteRows * lineHeight + padding * 2f;
            return height;
        }

        /// <summary>Top-left corner of a box of the given size centred in the screen.</summary>
        public static void Centre(float screenW, float screenH, float w, float h, out float x, out float y)
        {
            x = (screenW - w) * 0.5f;
            y = (screenH - h) * 0.5f;
        }

        /// <summary>Scale (at most 1) that makes a card of cardHeight fit in screenHeight with a margin each side.</summary>
        public static float FitScale(float cardHeight, float screenHeight, float margin)
        {
            var room = screenHeight - margin * 2f;
            if (cardHeight <= 0f || room <= 0f || cardHeight <= room) return 1f;
            return room / cardHeight;
        }

        /// <summary>How many lines a greedy word wrap needs when a line holds at most maxChars characters.</summary>
        public static int WrappedLineCount(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (maxChars < 1) maxChars = 1;
            var lines = 1;
            var used = 0;
            foreach (var word in text.Split(' '))
            {
                var length = word.Length;
                if (used == 0) used = length;
                else if (used + 1 + length <= maxChars) used += 1 + length;
                else { lines++; used = length; }
                while (used > maxChars) { lines++; used -= maxChars; }
            }
            return lines;
        }
    }
}
