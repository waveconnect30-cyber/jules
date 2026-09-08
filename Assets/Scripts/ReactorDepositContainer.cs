using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Server-authoritative IInteractable reactor container.
    /// Identifies sender via connectionToClient, verifies interaction distance on Server,
    /// and performs atomic transfer of carried Ignicita into reactor fuel.
    /// </summary>
    public class ReactorDepositContainer : NetworkBehaviour, IInteractable
    {
        [SerializeField] private int targetCityID = 1;
        [SerializeField] private float maxInteractionDistance = 4.0f;

        public string GetInteractionPrompt()
        {
            return "Presiona [F] para depositar Ignicita en el Reactor";
        }

        public void Interact(PlayerController player)
        {
            if (player == null || player.CarriedIgnicita <= 0f) return;
            CmdDepositIgnite();
        }

        [Command(requiresAuthority = false)]
        private void CmdDepositIgnite(NetworkConnectionToClient senderConn = null)
        {
            NetworkConnectionToClient conn = senderConn ?? connectionToClient;
            if (conn == null || conn.identity == null) return;

            PlayerController player = conn.identity.GetComponent<PlayerController>();
            if (player == null || player.IsDead) return;

            if (player.cityID != targetCityID)
            {
                Debug.LogWarning($"[ReactorDepositContainer SERVER] Rejected deposit: Player CityID:{player.cityID} != Reactor CityID:{targetCityID}");
                return;
            }

            // SERVER DISTANCE VALIDATION
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist > maxInteractionDistance)
            {
                Debug.LogWarning($"[ReactorDepositContainer SERVER] REJECTED deposit from {player.name}: Distance too far ({dist:F1}m > {maxInteractionDistance}m).");
                return;
            }

            float amountToDeposit = player.CarriedIgnicita;
            if (amountToDeposit <= 0f) return;

            // ATOMIC TRANSFER ON SERVER
            player.ConsumeCarriedIgnicita(amountToDeposit);

            ReactorManager reactor = GameManager.Instance != null ? GameManager.Instance.GetReactorForCity(targetCityID) : FindObjectOfType<ReactorManager>();
            if (reactor != null)
            {
                reactor.DepositIgnicita(amountToDeposit, targetCityID);
                Debug.Log($"[ReactorDepositContainer SERVER] ATOMIC DEPOSIT SUCCESS: Transferred {amountToDeposit} Ignicita from Player {player.name} to City {targetCityID} Reactor.");
            }
        }
    }
}
