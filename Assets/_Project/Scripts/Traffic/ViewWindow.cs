using UnityEngine;

namespace UncannyValet.Traffic
{
    /// <summary>The visible world rectangle of the camera, with the tests ambient traffic needs (who is on screen, where cars enter and leave).</summary>
    public readonly struct ViewWindow
    {
        /// <summary>The visible world rectangle.</summary>
        public readonly Rect Bounds;

        public ViewWindow(Rect bounds)
        {
            Bounds = bounds;
        }

        /// <summary>The window of an orthographic camera centred at <paramref name="centre"/> with half height <paramref name="halfHeight"/>.</summary>
        public static ViewWindow Of(Vector2 centre, float halfHeight, float aspect)
        {
            var half = new Vector2(halfHeight * aspect, halfHeight);
            return new ViewWindow(new Rect(centre - half, half * 2f));
        }

        /// <summary>True when <paramref name="point"/> is inside the window grown by <paramref name="margin"/> on every side.</summary>
        public bool Contains(Vector2 point, float margin) =>
            point.x >= Bounds.xMin - margin && point.x <= Bounds.xMax + margin &&
            point.y >= Bounds.yMin - margin && point.y <= Bounds.yMax + margin;

        /// <summary>True when a horizontal lane at height <paramref name="y"/> is inside the window's height grown by <paramref name="margin"/>.</summary>
        public bool CoversHeight(float y, float margin) => y >= Bounds.yMin - margin && y <= Bounds.yMax + margin;

        /// <summary>True when a car at <paramref name="x"/> has not yet reached the window, for a lane driving the given way.</summary>
        public bool IsUpstream(float x, bool leftward) => leftward ? x > Bounds.xMax : x < Bounds.xMin;

        /// <summary>True when a car at <paramref name="x"/> has left the window by more than <paramref name="margin"/>.</summary>
        public bool HasLeft(float x, bool leftward, float margin) => leftward ? x < Bounds.xMin - margin : x > Bounds.xMax + margin;

        /// <summary>The x where cars enter: just beyond the edge they approach from.</summary>
        public float EntryX(bool leftward, float margin) => leftward ? Bounds.xMax + margin : Bounds.xMin - margin;
    }
}
