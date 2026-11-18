using UncannyValet.Pedestrians;
using UnityEngine;

namespace UncannyValet.Bootstrap.Sandbox
{
    /// <summary>
    /// Puts the ambient crowd into a sandbox world: the pedestrians of the real game (walking the lot's sidewalks and crosswalks, in and out of the diner) with the world's own
    /// sprites. The crowd lives under the world, so a world switch removes it and the next world starts its own. Its size follows the F2 pedestrian count.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(SandboxWorldSlot))]
    public sealed class SandboxCrowd : MonoBehaviour
    {
        [SerializeField, Tooltip("The inactive crowd object under this world, with its PedestrianConfig. Set by the scene builder.")]
        private PedestrianCrowd _crowd;

        private void Start()
        {
            if (_crowd == null)
            {
                Debug.LogError("SandboxCrowd on " + name + " has no crowd; rebuild with Setup/Build Everything.", this);
                return;
            }
            PedestrianBootstrap.Start(_crowd, GetComponent<SandboxWorldSlot>().Decor);
        }
    }
}
