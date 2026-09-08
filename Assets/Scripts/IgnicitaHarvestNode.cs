using UnityEngine;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Concrete IInteractable node in the World Map.
    /// Allows players to harvest Ignicita mineral directly into carried inventory.
    /// </summary>
    public class IgnicitaHarvestNode : MonoBehaviour, IInteractable
    {
        [SerializeField] private float availableIgnicita = 100f;
        [SerializeField] private float harvestAmountPerPress = 25f;

        public string GetInteractionPrompt()
        {
            return $"Presiona [F] para extraer Ignicita ({availableIgnicita:F0} restante)";
        }

        public void Interact(PlayerController player)
        {
            if (player == null || availableIgnicita <= 0f) return;

            float harvested = Mathf.Min(availableIgnicita, harvestAmountPerPress);
            availableIgnicita -= harvested;
            player.AddCarriedIgnicita(harvested);

            Debug.Log($"[IgnicitaHarvestNode] Player {player.name} harvested {harvested} Ignicita. Remaining in node: {availableIgnicita}");
        }
    }
}
