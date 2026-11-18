using UncannyValet.Core.Contracts;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The mud field of a world without puddles. Registered before a world builds, so a lot with mud replaces it and one without leaves no earlier world's mud behind.</summary>
    public sealed class NoMudPatches : IMudPatches
    {
        public bool IsInside(Vector2 point) => false;
    }
}
