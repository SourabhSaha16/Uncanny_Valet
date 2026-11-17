namespace UncannyValet.UI.Speech
{
    /// <summary>Pure rule: is a speaker inside the camera view, so that its bubble is worth drawing (#129)?</summary>
    public static class SpeechBubbleVisibility
    {
        /// <summary>True when the viewport point (0..1 across the screen; depth is the distance in front of the camera) is in view, widened by <paramref name="margin"/>.</summary>
        public static bool IsInView(float viewportX, float viewportY, float depth, float margin)
        {
            if (depth < 0f) return false;
            var low = -margin;
            var high = 1f + margin;
            return viewportX >= low && viewportX <= high && viewportY >= low && viewportY <= high;
        }
    }
}
