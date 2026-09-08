using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Core
{
    /// <summary>
    /// Interface for interactive objects in the world (Reactor container, Wall repair stations, Resource crates).
    /// </summary>
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(PlayerController player);
    }
}
