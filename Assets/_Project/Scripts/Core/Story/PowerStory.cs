using System;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Core.Story
{
    /// <summary>The reward card lines of one power: what it does and the farewell of the world's god who grants it.</summary>
    [Serializable]
    public struct PowerStory
    {
        [SerializeField] private StoryPower _power;
        [SerializeField, Tooltip("What the power does, one or two lines (wrapped). Empty: the power definition's description is used.")]
        private string _description;
        [SerializeField, Tooltip("The farewell line of the god who grants it (wrapped). Said as a quote under the god's name.")]
        private string _farewell;

        public PowerStory(StoryPower power, string description, string farewell)
        {
            _power = power; _description = description; _farewell = farewell;
        }

        public StoryPower Power => _power;
        public string Description => _description;
        public string Farewell => _farewell;
    }
}
