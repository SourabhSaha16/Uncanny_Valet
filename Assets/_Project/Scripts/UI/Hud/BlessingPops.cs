using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// A small fixed pool of live blessing pops. Each remembers the world position it started at, so it keeps rising from there
    /// even if the patron or the car is gone. Starting a pop when full replaces the oldest. Nothing allocates after the
    /// text is formatted at <see cref="Start"/>.
    /// </summary>
    public sealed class BlessingPops
    {
        /// <summary>One floating amount.</summary>
        public struct Pop
        {
            public Vector2 World;
            public float StartedAt;
            public string Text;
            /// <summary>True when the lightning bolt is drawn after the text (every amount pop; not the PERFECT! label).</summary>
            public bool Bolt;
            public BlessingPopKind Kind;
            public bool Active;
        }

        private readonly Pop[] _pops;
        private readonly BlessingPopSettings _settings;

        public BlessingPops(BlessingPopSettings settings)
        {
            _settings = settings;
            _pops = new Pop[settings.MaxPops];
        }

        public int Capacity => _pops.Length;

        /// <summary>The pop in a slot (check <see cref="Pop.Active"/>).</summary>
        public Pop this[int index] => _pops[index];

        /// <summary>Number of pops currently alive.</summary>
        public int ActiveCount
        {
            get
            {
                var count = 0;
                for (var i = 0; i < _pops.Length; i++) if (_pops[i].Active) count++;
                return count;
            }
        }

        /// <summary>Starts a pop at a world position, formatting its text once.</summary>
        public void Start(Vector2 world, int blessing, float now)
        {
            _pops[FreeOrOldest()] = new Pop
            {
                World = world, StartedAt = now, Active = true,
                Text = BlessingPopText.Format(blessing), Bolt = true, Kind = BlessingPopText.KindOf(blessing, _settings.BigBlessingAmount)
            };
        }

        /// <summary>Starts a pop with ready-made text (the PERFECT! label), in the big look.</summary>
        public void StartLabel(Vector2 world, string text, float now)
        {
            _pops[FreeOrOldest()] = new Pop { World = world, StartedAt = now, Active = true, Text = text, Kind = BlessingPopKind.Big };
        }

        /// <summary>Retires pops that have run their course.</summary>
        public void Expire(float now)
        {
            for (var i = 0; i < _pops.Length; i++)
                if (_pops[i].Active && now - _pops[i].StartedAt >= _settings.DurationSeconds) _pops[i].Active = false;
        }

        /// <summary>Drops every pop.</summary>
        public void Clear()
        {
            for (var i = 0; i < _pops.Length; i++) _pops[i].Active = false;
        }

        private int FreeOrOldest()
        {
            var oldest = 0;
            for (var i = 0; i < _pops.Length; i++)
            {
                if (!_pops[i].Active) return i;
                if (_pops[i].StartedAt < _pops[oldest].StartedAt) oldest = i;
            }
            return oldest;
        }
    }
}
