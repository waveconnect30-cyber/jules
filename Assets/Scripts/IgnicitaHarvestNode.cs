using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Server-authoritative IInteractable mineral node.
    /// Validates player distance on Server before harvesting Ignicita into player SyncVar inventory.
    /// </summary>
    public class IgnicitaHarvestNode : NetworkBehaviour, IInteractable
    {
        [SyncVar] [SerializeField] private float availableIgnicita = 100f;
        [SerializeField] private float harvestAmountPerPress = 25f;
        [SerializeField] private float maxHarvestDistance = 4.0f;

        public string GetInteractionPrompt()
        {
            return $"Presiona [F] para extraer Ignicita ({availableIgnicita:F0} restante)";
        }

        public void Interact(PlayerController player)
        {
            if (player == null || availableIgnicita <= 0f) return;
            CmdHarvestMineral();
        }

        [Command(requiresAuthority = false)]
        private void CmdHarvestMineral(NetworkConnectionToClient senderConn = null)
        {
            NetworkConnectionToClient conn = senderConn ?? connectionToClient;
            if (conn == null || conn.identity == null) return;

            PlayerController player = conn.identity.GetComponent<PlayerController>();
            if (player == null || player.IsDead) return;

            // SERVER DISTANCE VALIDATION
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist > maxHarvestDistance)
            {
                Debug.LogWarning($"[IgnicitaHarvestNode SERVER] REJECTED harvest from {player.name}: Distance too far ({dist:F1}m > {maxHarvestDistance}m).");
                return;
            }

            if (availableIgnicita <= 0f) return;

            float harvested = Mathf.Min(availableIgnicita, harvestAmountPerPress);
            availableIgnicita -= harvested;
            player.AddCarriedIgnicita(harvested);

            Debug.Log($"[IgnicitaHarvestNode SERVER] Player {player.name} harvested {harvested} Ignicita. Remaining in node: {availableIgnicita}");
        }
    }
}
