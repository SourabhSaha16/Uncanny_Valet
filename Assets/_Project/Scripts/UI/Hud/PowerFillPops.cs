using UncannyValet.Core.Events;

namespace UncannyValet.UI.Hud
{
    /// <summary>
    /// A small fixed pool of live "+X" power pops. A new pop takes the lowest stack slot that no recent pop still holds, so pops
    /// arriving together sit one under another instead of on top of each other. Full pool: the oldest is replaced.
    /// </summary>
    public sealed class PowerFillPops
    {
        /// <summary>One floating amount.</summary>
        public struct Pop
        {
            public float StartedAt;
            public int Slot;
            public string Text;
            public PowerFillSource Source;
            public bool Active;
        }

        private readonly Pop[] _pops;
        private readonly PowerFillPopSettings _settings;

        public PowerFillPops(PowerFillPopSettings settings)
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

        /// <summary>Starts a pop for an amount; false (and nothing shown) when it rounds to zero.</summary>
        public bool Start(float amount, PowerFillSource source, float now)
        {
            if (!PowerFillPopRule.TryFormat(amount, out var text)) return false;
            var slot = FreeSlot(now);
            _pops[FreeOrOldest()] = new Pop { StartedAt = now, Slot = slot, Text = text, Source = source, Active = true };
            return true;
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

        private int FreeSlot(float now)
        {
            for (var slot = 0; slot < _pops.Length; slot++)
                if (!SlotHeld(slot, now)) return slot;
            return _pops.Length - 1;
        }

        private bool SlotHeld(int slot, float now)
        {
            for (var i = 0; i < _pops.Length; i++)
                if (_pops[i].Active && _pops[i].Slot == slot && now - _pops[i].StartedAt < _settings.SlotHoldSeconds) return true;
            return false;
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
