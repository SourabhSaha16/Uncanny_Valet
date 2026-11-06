using UnityEngine;

namespace UncannyValet.Core.Contracts
{
    /// <summary>Anything the valet can use with the interact key (cars now, stations later).</summary>
    public interface IInteractable
    {
        /// <summary>Short verb shown in the debug overlay, e.g. "Enter car".</summary>
        string Prompt { get; }

        Vector2 Position { get; }

        bool CanInteract(GameObject actor);

        void Interact(GameObject actor);
    }
}
