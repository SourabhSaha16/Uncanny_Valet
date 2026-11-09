using UnityEngine;

namespace UncannyValet.Pedestrians
{
    /// <summary>Pure rules for a pedestrian's walk animation: which way they face and which frame shows.</summary>
    public static class WalkAnimMath
    {
        /// <summary>Facing for a heading: the dominant axis wins. A zero or exactly diagonal heading keeps the current facing.</summary>
        public static WalkFacing Facing(Vector2 heading, WalkFacing current)
        {
            var ax = Mathf.Abs(heading.x);
            var ay = Mathf.Abs(heading.y);
            if (Mathf.Approximately(ax, ay)) return current;
            if (ax > ay) return heading.x > 0f ? WalkFacing.Right : WalkFacing.Left;
            return heading.y > 0f ? WalkFacing.Up : WalkFacing.Down;
        }

        /// <summary>Run-cycle frame 0..frames-1 for the time spent walking. Returns 0 when frames is 0 or less.</summary>
        public static int Frame(float walkingSeconds, float framesPerSecond, int frames)
        {
            if (frames <= 0 || walkingSeconds <= 0f) return 0;
            return (int)(walkingSeconds * framesPerSecond) % frames;
        }
    }
}
