using UnityEngine;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// A small fixed pool of live tip pops. Each remembers the world position it started at, so it keeps rising from there
    /// even if the patron or the car is gone. Starting a pop when full replaces the oldest. Nothing allocates after the
    /// text is formatted at <see cref="Start"/>.
    /// </summary>
    public sealed class TipPops
    {
        /// <summary>One floating amount.</summary>
        public struct Pop
        {
            public Vector2 World;
            public float StartedAt;
            public string Text;
            public TipPopKind Kind;
            public bool Active;
        }

        private readonly Pop[] _pops;
        private readonly TipPopSettings _settings;

        public TipPops(TipPopSettings settings)
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
        public void Start(Vector2 world, int tip, float now)
        {
            _pops[FreeOrOldest()] = new Pop
            {
                World = world, StartedAt = now, Active = true,
                Text = TipPopText.Format(tip), Kind = TipPopText.KindOf(tip, _settings.BigTipDollars)
            };
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
