using System.Collections.Generic;
using UncannyValet.Core;
using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>
    /// Shows one sandbox world at a time. A switch destroys the world on screen and clones the next template, so its lot builds from scratch
    /// (walls, mud puddles, props, lighting) exactly as a real lot scene does, then re-registers what the lot provides (layout, mud) and refits the camera.
    /// </summary>
    public sealed class SandboxWorldHost
    {
        private const float BoundaryThickness = 2f;

        private readonly Transform _parent;
        private readonly IReadOnlyList<SandboxWorldSlot> _slots;
        private readonly CameraFollow _camera;
        private SandboxWorldSlot _shown;

        public SandboxWorldHost(Transform parent, IReadOnlyList<SandboxWorldSlot> slots, CameraFollow camera)
        {
            _parent = parent;
            _slots = slots;
            _camera = camera;
        }

        // Deactivated first: Destroy only takes effect at the end of the frame, and two global lights at once is an error of the 2D renderer.
        private static void Retire(SandboxWorldSlot world)
        {
            world.gameObject.SetActive(false);
            Object.Destroy(world.gameObject);
        }

        /// <summary>The layout of the world on screen; null before the first <see cref="Show"/>.</summary>
        public ILotLayout Layout { get; private set; }

        /// <summary>Index of the template on screen; -1 before the first <see cref="Show"/>.</summary>
        public int ShownIndex { get; private set; } = -1;

        /// <summary>Replaces the world on screen with the template at <paramref name="index"/> and returns its layout (also registered in <see cref="Services"/>).</summary>
        public ILotLayout Show(int index)
        {
            if (_shown != null) Retire(_shown);
            Services.Register<IMudPatches>(new NoMudPatches()); // a lot with mud replaces this while it builds
            _shown = Object.Instantiate(_slots[index], _parent);
            _shown.gameObject.SetActive(true);
            var source = _shown.Source;
            LotRegistration.RegisterAll(source);
            if (!LotBounds.TryMeasure(source.Root, out var area)) Debug.LogError("The sandbox lot " + source.Root.name + " has no renderers; the camera cannot frame it.", source.Root);
            else
            {
                SandboxBoundary.Build(_shown.transform, area, BoundaryThickness);
                _camera.SetWorldBounds(source.ExtendView(area), area);
            }
            Layout = source.Layout;
            ShownIndex = index;
            return Layout;
        }
    }
}
