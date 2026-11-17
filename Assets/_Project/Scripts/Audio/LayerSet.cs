using System.Collections.Generic;

namespace UncannyValet.Audio
{
    /// <summary>
    /// Every level sound layer the director holds. A level change calls <see cref="SwitchTo"/>: each held layer is
    /// silenced (stopped, volume 0, clip released) before the new theme's layers are held, so nothing of the previous
    /// level survives and repeated changes never accumulate.
    /// </summary>
    public sealed class LayerSet
    {
        private readonly List<IAudioLayer> _held = new List<IAudioLayer>();

        /// <summary>How many layers are held and not silenced.</summary>
        public int ActiveCount
        {
            get
            {
                var count = 0;
                for (int i = 0; i < _held.Count; i++) if (!_held[i].IsSilenced) count++;
                return count;
            }
        }

        /// <summary>Holds a layer of the current theme.</summary>
        public void Hold(IAudioLayer layer) => _held.Add(layer);

        /// <summary>Silences every held layer and forgets them.</summary>
        public void SilenceAll()
        {
            for (int i = 0; i < _held.Count; i++) _held[i].Silence();
            _held.Clear();
        }

        /// <summary>Silences everything held, then holds the layers of the new theme (the same theme again restarts clean).</summary>
        public void SwitchTo(IReadOnlyList<IAudioLayer> layers)
        {
            SilenceAll();
            for (int i = 0; i < layers.Count; i++) _held.Add(layers[i]);
        }
    }
}
