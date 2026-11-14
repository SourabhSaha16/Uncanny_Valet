using System.Collections.Generic;
using UncannyValet.Core.Art;
using UnityEngine;

namespace UncannyValet.Lot
{
    /// <summary>Drives the turning and blinking layers of a lot's props from one component; reuses its list, so nothing is allocated per frame.</summary>
    [DisallowMultipleComponent]
    public sealed class PropAnimator : MonoBehaviour
    {
        private struct Part
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public PropAnimation Animation;
            public float Speed;
        }

        private readonly List<Part> _parts = new List<Part>();

        /// <summary>Number of animated layers being driven.</summary>
        public int Count => _parts.Count;

        /// <summary>Starts driving a layer; layers without an animation are ignored.</summary>
        public void Add(SpriteRenderer renderer, PropAnimation animation, float speed)
        {
            if (animation == PropAnimation.None) return;
            _parts.Add(new Part { Transform = renderer.transform, Renderer = renderer, Animation = animation, Speed = speed });
        }

        private void Update() => Tick(Time.time);

        /// <summary>Moves every animated layer to where it is at <paramref name="time"/> seconds.</summary>
        public void Tick(float time)
        {
            for (var i = 0; i < _parts.Count; i++)
            {
                var part = _parts[i];
                if (part.Animation == PropAnimation.Spin) part.Transform.localRotation = Quaternion.Euler(0f, 0f, PropMotion.SpinAngle(time, part.Speed));
                else part.Renderer.enabled = PropMotion.BlinkOn(time, part.Speed);
            }
        }
    }
}
