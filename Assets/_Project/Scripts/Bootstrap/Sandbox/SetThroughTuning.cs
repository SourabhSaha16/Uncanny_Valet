using System;
using UncannyValet.Core.Contracts;
using UncannyValet.Core.Events;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>
    /// An <see cref="ITuning"/> over the live store whose Seed writes with Set: code that seeds a lot's numbers (which normally runs once at
    /// start) can run again mid-play and the change is published. The store's own seeds, which Reset returns to, stay as they were.
    /// </summary>
    public sealed class SetThroughTuning : ITuning
    {
        private readonly ITuning _inner;

        public SetThroughTuning(ITuning inner) { _inner = inner ?? throw new ArgumentNullException(nameof(inner)); }

        public float Get(TuningKey key) => _inner.Get(key);

        public bool TryGet(TuningKey key, out float value) => _inner.TryGet(key, out value);

        public void Set(TuningKey key, float value) => _inner.Set(key, value);

        public void Seed(TuningKey key, float value) => _inner.Set(key, value);

        public void Reset(TuningKey key) => _inner.Reset(key);
    }
}
