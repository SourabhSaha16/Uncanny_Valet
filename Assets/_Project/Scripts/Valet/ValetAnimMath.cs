using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>Pure rules for the valet's walk animation: which way he faces and which frame shows.</summary>
    public static class ValetAnimMath
    {
        /// <summary>Facing for a velocity. Below <paramref name="minSpeed"/> or on an exact diagonal the current facing stays.</summary>
        public static ValetFacing Facing(Vector2 velocity, ValetFacing current, float minSpeed)
        {
            if (velocity.sqrMagnitude < minSpeed * minSpeed) return current;
            var ax = Mathf.Abs(velocity.x);
            var ay = Mathf.Abs(velocity.y);
            if (Mathf.Approximately(ax, ay)) return current;
            if (ax > ay) return velocity.x > 0f ? ValetFacing.Right : ValetFacing.Left;
            return velocity.y > 0f ? ValetFacing.Up : ValetFacing.Down;
        }

        /// <summary>Run-cycle frame 0..frames-1 for the time spent moving. Returns 0 when frames is 0 or less.</summary>
        public static int Frame(float movingSeconds, float framesPerSecond, int frames)
        {
            if (frames <= 0 || movingSeconds <= 0f) return 0;
            return (int)(movingSeconds * framesPerSecond) % frames;
        }

        /// <summary>Sheet column of a frame: the first column of the facing's group plus the frame.</summary>
        public static int Column(int groupStart, int frame) => groupStart + frame;
    }
}
