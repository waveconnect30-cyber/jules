using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Server-authoritative IInteractable reactor container.
    /// Identifies sender via connectionToClient, verifies interaction distance on Server,
    /// validates reactor capacity BEFORE deducting player fuel, and preserves any unconsumed remainder.
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

            // 1. SERVER DISTANCE VALIDATION
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist > maxInteractionDistance)
            {
                Debug.LogWarning($"[ReactorDepositContainer SERVER] REJECTED deposit from {player.name}: Distance too far ({dist:F1}m > {maxInteractionDistance}m). Player resources preserved.");
                return;
            }

            // 2. RESOLVE REACTOR FIRST BEFORE DEDUCTING RESOURCES
            ReactorManager reactor = GameManager.Instance != null ? GameManager.Instance.GetReactorForCity(targetCityID) : FindObjectOfType<ReactorManager>();
            if (reactor == null)
            {
                Debug.LogWarning($"[ReactorDepositContainer SERVER] REJECTED deposit: Reactor for City {targetCityID} not found. Player resources preserved.");
                return;
            }

            // 3. CALCULATE ACCEPTED CAPACITY BEFORE CONSUMPTION
            float availableCapacity = Mathf.Max(0f, reactor.MaxIgnicita - reactor.CurrentIgnicita);
            if (availableCapacity <= 0f)
            {
                Debug.LogWarning($"[ReactorDepositContainer SERVER] REJECTED deposit: Reactor for City {targetCityID} is already at max fuel capacity ({reactor.CurrentIgnicita}/{reactor.MaxIgnicita}). Player resources preserved.");
                return;
            }

            float carried = player.CarriedIgnicita;
            float amountToDeposit = Mathf.Min(carried, availableCapacity);

            if (amountToDeposit <= 0f) return;

            // 4. DEDUCT ONLY THE ACCEPTED AMOUNT AND DEPOSIT ATOMICALLY
            player.ConsumeCarriedIgnicita(amountToDeposit);
            reactor.DepositIgnicita(amountToDeposit, targetCityID);

            Debug.Log($"[ReactorDepositContainer SERVER] ATOMIC DEPOSIT SUCCESS: Transferred {amountToDeposit} Ignicita from Player {player.name} (Carried remaining: {player.CarriedIgnicita}) to City {targetCityID} Reactor ({reactor.CurrentIgnicita}/{reactor.MaxIgnicita}).");
        }
    }
}
