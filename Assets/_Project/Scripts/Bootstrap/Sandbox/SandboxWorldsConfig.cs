using System.Collections.Generic;
using UncannyValet.Run;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>The worlds the sandbox offers, in picker order (key 1 is the first). Adding a world is adding a lot definition here and running Uncanny Valet/Setup/Build Everything. Read-only at runtime.</summary>
    [CreateAssetMenu(menuName = "Uncanny Valet/Sandbox Worlds", fileName = "SandboxWorldsConfig")]
    public sealed class SandboxWorldsConfig : ScriptableObject
    {
        [SerializeField, Tooltip("One lot definition per world; its walls, ground, props, lighting and mud are what the car drives on.")]
        private List<LotDefinition> _worlds = new List<LotDefinition>();

        public IReadOnlyList<LotDefinition> Worlds => _worlds;
    }
}
