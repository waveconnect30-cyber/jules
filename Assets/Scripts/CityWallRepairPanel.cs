using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Server-authoritative IInteractable wall repair station.
    /// Validates interaction distance on Server, verifies and deducts warehouse Steel/Ignicita,
    /// and restores wall section HP.
    /// </summary>
    public class CityWallRepairPanel : NetworkBehaviour, IInteractable
    {
        [SerializeField] private int targetCityID = 1;
        [SerializeField] private WallSection sectionToRepair = WallSection.North;
        [SerializeField] private float repairHPBonus = 250f;
        [SerializeField] private int requiredSteelCost = 10;
        [SerializeField] private float maxInteractionDistance = 4.0f;

        public string GetInteractionPrompt()
        {
            return $"Presiona [F] para reparar Muralla {sectionToRepair} (+{repairHPBonus} HP, Costo: {requiredSteelCost} Acero)";
        }

        public void Interact(PlayerController player)
        {
            if (player == null) return;
            CmdRepairWall();
        }

        [Command(requiresAuthority = false)]
        private void CmdRepairWall(NetworkConnectionToClient senderConn = null)
        {
            NetworkConnectionToClient conn = senderConn ?? connectionToClient;
            if (conn == null || conn.identity == null) return;

            PlayerController player = conn.identity.GetComponent<PlayerController>();
            if (player == null || player.IsDead) return;

            if (player.cityID != targetCityID)
            {
                Debug.LogWarning($"[CityWallRepairPanel SERVER] Rejected repair attempt by enemy player CityID:{player.cityID}");
                return;
            }

            // SERVER DISTANCE VALIDATION
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist > maxInteractionDistance)
            {
                Debug.LogWarning($"[CityWallRepairPanel SERVER] REJECTED repair from {player.name}: Distance too far ({dist:F1}m > {maxInteractionDistance}m).");
                return;
            }

            // WAREHOUSE MATERIAL CHECK & DEDUCTION
            SharedInventorySync warehouse = GameManager.Instance != null ? GameManager.Instance.GetWarehouseForCity(targetCityID) : FindObjectOfType<SharedInventorySync>();
            if (warehouse == null || !warehouse.ConsumeSteelFromWarehouse(requiredSteelCost, targetCityID))
            {
                Debug.LogWarning($"[CityWallRepairPanel SERVER] REJECTED repair: City {targetCityID} warehouse lacks sufficient Steel ({requiredSteelCost} required).");
                return;
            }

            // REPAIR WALL
            CityWallHealthSync wall = GameManager.Instance != null ? GameManager.Instance.GetWallForCity(targetCityID) : FindObjectOfType<CityWallHealthSync>();
            if (wall != null)
            {
                wall.RepairWall(sectionToRepair, repairHPBonus, targetCityID);
                Debug.Log($"[CityWallRepairPanel SERVER] WALL REPAIRED: Section {sectionToRepair} +{repairHPBonus} HP on City {targetCityID}. Consumed {requiredSteelCost} Steel.");
            }
        }
    }
}
