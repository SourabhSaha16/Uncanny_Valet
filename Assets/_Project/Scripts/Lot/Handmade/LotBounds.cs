using UnityEngine;

namespace UncannyValet.Lot.Handmade
{
    /// <summary>Pure extents of an authored lot: the box round its gameplay markers and the rectangle the camera shows.</summary>
    public static class LotBounds
    {
        /// <summary>The smallest rectangle holding every bay, the curb, the queue slots, the doors, the wait points and the valet start; empty when there are none.</summary>
        public static Rect Gameplay(HandmadeLotData d)
        {
            var box = new Bounder();
            for (var i = 0; i < d.Bays.Count; i++) box.Add(d.Bays[i].Rect);
            for (var i = 0; i < d.Curbs.Count; i++) box.Add(d.Curbs[i].Rect);
            for (var i = 0; i < d.Slots.Count; i++) box.Add(d.Slots[i].Rect);
            for (var i = 0; i < d.Doors.Count; i++) box.Add(d.Doors[i].Position);
            for (var i = 0; i < d.WaitPoints.Count; i++) box.Add(d.WaitPoints[i].Position);
            for (var i = 0; i < d.ValetStarts.Count; i++) box.Add(d.ValetStarts[i].Position);
            return box.Result;
        }

        /// <summary>What the camera shows: the authored view when there is one, otherwise the gameplay box.</summary>
        public static Rect View(HandmadeLotData d) => d.HasView ? d.View : Gameplay(d);

        private struct Bounder
        {
            private Vector2 _min;
            private Vector2 _max;
            private bool _any;

            public Rect Result => _any ? Rect.MinMaxRect(_min.x, _min.y, _max.x, _max.y) : default;

            public void Add(OrientedRect r)
            {
                for (var i = 0; i < 4; i++) Add(r.Corner(i));
            }

            public void Add(Vector2 p)
            {
                if (!_any) { _min = p; _max = p; _any = true; return; }
                _min = Vector2.Min(_min, p);
                _max = Vector2.Max(_max, p);
            }
        }
    }
}
