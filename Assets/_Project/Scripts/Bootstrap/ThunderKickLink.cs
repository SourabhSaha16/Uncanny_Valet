using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;
using UnityEngine;

namespace UncannyValet.Bootstrap
{
    /// <summary>Turns ThunderStrideStarted into a small camera kick along the dash or boost direction.</summary>
    public sealed class ThunderKickLink : IDisposable
    {
        private readonly IEventBus _bus;
        private readonly Action<Vector2> _kick;

        /// <param name="kick">Receives the unit direction of the dash or boost.</param>
        public ThunderKickLink(IEventBus bus, Action<Vector2> kick)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _kick = kick ?? throw new ArgumentNullException(nameof(kick));
            _bus.Subscribe<ThunderStrideStarted>(OnStarted);
        }

        /// <summary>Stops listening.</summary>
        public void Dispose() => _bus.Unsubscribe<ThunderStrideStarted>(OnStarted);

        private void OnStarted(ThunderStrideStarted e) => _kick(e.Direction);
    }
}
