using System;
using System.Collections.Generic;

namespace UncannyValet.Core
{
    /// <summary>
    /// The one registry in the game. The bootstrap registers every service at scene start;
    /// tests call <see cref="Reset"/> in their setup. Modules resolve contracts, never concrete types.
    /// </summary>
    public static class Services
    {
        private static readonly Dictionary<Type, object> Registry = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            Registry[typeof(T)] = service ?? throw new ArgumentNullException(nameof(service));
        }

        public static T Get<T>() where T : class
        {
            if (Registry.TryGetValue(typeof(T), out var service)) return (T)service;
            throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var found = Registry.TryGetValue(typeof(T), out var raw);
            service = found ? (T)raw : null;
            return found;
        }

        public static void Reset() => Registry.Clear();
    }
}
