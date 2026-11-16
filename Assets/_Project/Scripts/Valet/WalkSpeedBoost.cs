using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UncannyValet.Core.Ids;
using UnityEngine;

namespace UncannyValet.Valet
{
    /// <summary>
    /// Thunder stride on foot: while the power runs the valet's walking speed is multiplied by the level's number (the
    /// Secondary field). Set, never multiplied up, so a refired power cannot stack; back to 1 when the power ends.
    /// </summary>
    public sealed class WalkSpeedBoost : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly Func<IPowerView> _view;

        /// <param name="view">Resolves the power numbers; may return null before the Powers module has registered them.</param>
        public WalkSpeedBoost(IEventBus bus, Func<IPowerView> view)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _bus.Subscribe<PowerActivated>(OnActivated);
            _bus.Subscribe<PowerEnded>(OnEnded);
        }

        /// <summary>The factor on the walking speed: 1 normally, the power's number while it runs.</summary>
        public float Multiplier { get; private set; } = 1f;

        /// <summary>Stops listening and ends any boost.</summary>
        public void Dispose()
        {
            _bus.Unsubscribe<PowerActivated>(OnActivated);
            _bus.Unsubscribe<PowerEnded>(OnEnded);
            Multiplier = 1f;
        }

        private void OnActivated(PowerActivated e)
        {
            if (e.Power != StoryPower.ThunderStride) return;
            var view = _view();
            if (view == null || !view.TryGetValues(e.Power, e.Level, out var values)) return;
            Multiplier = Mathf.Max(1f, values.Secondary);
        }

        private void OnEnded(PowerEnded e)
        {
            if (e.Power == StoryPower.ThunderStride) Multiplier = 1f;
        }
    }
}
