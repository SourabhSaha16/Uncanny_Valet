using System;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Story
{
    /// <summary>The look and name of one world's story cards: its name and two accent colours.</summary>
    [Serializable]
    public struct WorldStory
    {
        [SerializeField] private World _world;
        [SerializeField, Tooltip("Shown big on the intro card, uppercase (MIDGARD).")] private string _worldName;
        [SerializeField, Tooltip("Main accent colour of the world's cards (headings, the top bar).")] private Color _accent;
        [SerializeField, Tooltip("Second accent colour (the bottom bar, the frame details).")] private Color _accentSecond;

        public WorldStory(World world, string worldName, Color accent, Color accentSecond)
        {
            _world = world; _worldName = worldName; _accent = accent; _accentSecond = accentSecond;
        }

        public World World => _world;
        public string WorldName => _worldName;
        public Color Accent => _accent;
        public Color AccentSecond => _accentSecond;
    }
}
