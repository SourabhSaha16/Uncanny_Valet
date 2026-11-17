using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// Pure rule of the curse pop: how many blessings a curse really cost (the total never goes below 0) and the text shown.
    /// Also pairs the two events a pedestrian curse needs, because the curse (amount) and the hit (position) arrive in either
    /// order within the same frame: the pair completes when both are present for one frame.
    /// </summary>
    public sealed class CursePopRule
    {
        private bool _hasHit;
        private Vector2 _point;
        private int _hitFrame;
        private bool _hasCurse;
        private int _lost;
        private int _curseFrame;

        /// <summary>Blessings a curse of <paramref name="amount"/> really takes off a total of <paramref name="blessings"/>.</summary>
        public static int Lost(int blessings, int amount) => Mathf.Max(0, Mathf.Min(blessings, amount));

        /// <summary>The label: "-N".</summary>
        public static string Format(int lost) => "-" + lost;

        /// <summary>The driven car hit someone at <paramref name="point"/>. True when this completes a pop (read it with <see cref="Take"/>).</summary>
        public bool Hit(Vector2 point, int frame)
        {
            _hasHit = true;
            _point = point;
            _hitFrame = frame;
            return Ready;
        }

        /// <summary>A curse without a ticket took <paramref name="lost"/> blessings. Nothing lost starts no pop. True when this completes a pop.</summary>
        public bool Curse(int lost, int frame)
        {
            if (lost <= 0) return false;
            _hasCurse = true;
            _lost = lost;
            _curseFrame = frame;
            return Ready;
        }

        /// <summary>Reads and clears the completed pair.</summary>
        public void Take(out int lost, out Vector2 point)
        {
            lost = _lost;
            point = _point;
            _hasHit = false;
            _hasCurse = false;
        }

        /// <summary>Forgets a half-finished pair.</summary>
        public void Clear()
        {
            _hasHit = false;
            _hasCurse = false;
        }

        private bool Ready => _hasHit && _hasCurse && _hitFrame == _curseFrame;
    }
}
