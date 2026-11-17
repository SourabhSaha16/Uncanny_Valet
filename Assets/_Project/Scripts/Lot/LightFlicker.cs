using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UncannyValet.Lot
{
    /// <summary>Makes the flickering point lights of a lot's lighting dim and brighten; one component drives them all and allocates nothing per frame.</summary>
    [DisallowMultipleComponent]
    public sealed class LightFlicker : MonoBehaviour
    {
        private struct Flame
        {
            public Light2D Light;
            public float Base;
            public float Depth;
            public float Speed;
            public float Seed;
        }

        private readonly List<Flame> _flames = new List<Flame>();

        /// <summary>Number of flickering lights.</summary>
        public int Count => _flames.Count;

        /// <summary>Starts flickering <paramref name="light"/> around its current intensity.</summary>
        public void Add(Light2D light, float depth, float speed)
            => _flames.Add(new Flame { Light = light, Base = light.intensity, Depth = depth, Speed = speed, Seed = _flames.Count * 3.7f });

        private void Update() => Tick(Time.time);

        /// <summary>Sets every flickering light to its strength at <paramref name="time"/> seconds.</summary>
        public void Tick(float time)
        {
            for (var i = 0; i < _flames.Count; i++)
            {
                var flame = _flames[i];
                flame.Light.intensity = flame.Base * PropMotion.Flicker(time, flame.Speed, flame.Depth, flame.Seed);
            }
        }
    }
}
