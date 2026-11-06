using System;
using System.Collections.Generic;

namespace UncannyValet.Core.Events
{
    /// <summary>
    /// Plain C# event bus. Created once by the bootstrap and registered in <see cref="Services"/>.
    /// Tests can create their own instance with no scene.
    /// </summary>
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _handlers.TryGetValue(typeof(T), out var existing);
            _handlers[typeof(T)] = Delegate.Combine(existing, handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null || !_handlers.TryGetValue(typeof(T), out var existing)) return;
            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null) _handlers.Remove(typeof(T));
            else _handlers[typeof(T)] = remaining;
        }

        public void Publish<T>(T gameEvent) where T : struct, IGameEvent
        {
            if (_handlers.TryGetValue(typeof(T), out var existing))
            {
                ((Action<T>)existing).Invoke(gameEvent);
            }
        }
    }
}
