using UncannyValet.Core.Contracts;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Powers
{
    /// <summary>
    /// One story power: its name, one-line description, icon, sound sting and the numbers of its two levels (base and
    /// upgraded). Read-only at runtime; the live numbers follow the F2 panel through <see cref="PowerSettings"/>.
    /// The shared power bar is the charge system, so there are no per-power charges.
    /// </summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Power Definition", fileName = "PowerDefinition")]
    public sealed class PowerDefinition : ScriptableObject
    {
        [SerializeField] private StoryPower _power = StoryPower.ThunderStride;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(1, 2), Tooltip("One line, shown on the reward card.")]
        private string _description;
        [SerializeField, Tooltip("Optional art. The HUD draws a pixel icon itself when this is empty.")]
        private Sprite _icon;
        [SerializeField, Tooltip("Optional sound sting played when the power starts.")]
        private AudioClip _sting;
        [SerializeField] private PowerLevelData _level1 = new PowerLevelData();
        [SerializeField] private PowerLevelData _level2 = new PowerLevelData();

        public StoryPower Power => _power;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public AudioClip Sting => _sting;

        /// <summary>The authored numbers of a level (1 base, 2 upgraded); false for any other level.</summary>
        public bool TryGetValues(int level, out PowerLevelValues values)
        {
            values = default;
            if (level == 1) values = _level1.ToValues();
            else if (level == 2) values = _level2.ToValues();
            return level == 1 || level == 2;
        }

        /// <summary>Fills a new definition with the shipped defaults of the power (setup tools and tests).</summary>
        public void ApplyDefaults(StoryPower power)
        {
            _power = power;
            _displayName = PowerDefaults.DisplayName(power);
            _description = PowerDefaults.Description(power);
            _level1 = new PowerLevelData(PowerDefaults.Values(power, 1));
            _level2 = new PowerLevelData(PowerDefaults.Values(power, 2));
        }
    }
}
