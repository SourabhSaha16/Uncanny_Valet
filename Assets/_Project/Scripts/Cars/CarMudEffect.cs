using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Cars
{
    /// <summary>
    /// Slows the car while it drives through mud. A driven car gets a <see cref="MudModifier"/> on its modifier stack; a game-driven car
    /// (see <see cref="CarAutoDrive"/>) reads <see cref="SpeedScale"/>. Only cars: the valet on foot and the pedestrians never see mud.
    /// Mud never touches the car's service modifiers, so it cannot make a washed car Muddy.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CarDriver))]
    [DisallowMultipleComponent]
    public sealed class CarMudEffect : MonoBehaviour
    {
        private readonly MudBlend _blend = new MudBlend();
        private Rigidbody2D _body;
        private CarDriver _driver;
        private MudModifier _modifier;
        private IMudPatches _patches;
        private ITuning _tuning;

        /// <summary>1 clear of mud; the top speed multiplier while in mud (fading back after leaving).</summary>
        public float SpeedScale => _modifier != null ? _modifier.SpeedScale : 1f;

        /// <summary>0 to 1: how much mud the car feels right now.</summary>
        public float Weight => _blend.Weight;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _driver = GetComponent<CarDriver>();
            _modifier = new MudModifier(_blend, MudSettings.Defaults);
            _driver.Modifiers.Add(_modifier);
        }

        private void Start()
        {
            Services.TryGet(out _patches);
            Services.TryGet(out _tuning);
        }

        private void OnDestroy() => _driver.Modifiers.Remove(_modifier);

        private void FixedUpdate()
        {
            if (_patches == null || _driver.Definition == null) return;
            var settings = MudSettings.Read(_tuning);
            _modifier.Settings = settings;
            var inside = CarMudProbe.IsInMud(_patches, _body.position, transform.up, _driver.Definition.Size);
            _blend.Step(inside, Time.fixedDeltaTime, settings.ReleaseSeconds);
        }
    }
}
