using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Server-authoritative IInteractable wall repair station.
    /// Resolves target wall and verifies section HP < maxHP BEFORE deducting Steel from warehouse,
    /// preserving warehouse resources if repair is invalid or wall is intact.
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

            // 1. SERVER DISTANCE VALIDATION
            float dist = Vector3.Distance(player.transform.position, transform.position);
            if (dist > maxInteractionDistance)
            {
                Debug.LogWarning($"[CityWallRepairPanel SERVER] REJECTED repair from {player.name}: Distance too far ({dist:F1}m > {maxInteractionDistance}m). Materials preserved.");
                return;
            }

            // 2. RESOLVE WALL TARGET AND VERIFY NEED FOR REPAIR BEFORE CHARGING STEEL
            CityWallHealthSync wall = GameManager.Instance != null ? GameManager.Instance.GetWallForCity(targetCityID) : FindObjectOfType<CityWallHealthSync>();
            if (wall == null)
            {
                Debug.LogWarning($"[CityWallRepairPanel SERVER] REJECTED repair: Wall for City {targetCityID} not found. Materials preserved.");
                return;
            }

            WallStatus wallStatus = wall.GetWallStatus(sectionToRepair);
            if (wallStatus.currentHP >= wallStatus.maxHP)
            {
                Debug.LogWarning($"[CityWallRepairPanel SERVER] REJECTED repair: Section {sectionToRepair} is already at max HP ({wallStatus.currentHP}/{wallStatus.maxHP}). Materials preserved.");
                return;
            }

            // 3. WAREHOUSE MATERIAL CHECK AND DEDUCTION ONLY AFTER WALL VALIDATION
            SharedInventorySync warehouse = GameManager.Instance != null ? GameManager.Instance.GetWarehouseForCity(targetCityID) : FindObjectOfType<SharedInventorySync>();
            if (warehouse == null || !warehouse.ConsumeSteelFromWarehouse(requiredSteelCost, targetCityID))
            {
                Debug.LogWarning($"[CityWallRepairPanel SERVER] REJECTED repair: City {targetCityID} warehouse lacks sufficient Steel ({requiredSteelCost} required).");
                return;
            }

            // 4. APPLY REPAIR
            wall.RepairWall(sectionToRepair, repairHPBonus, targetCityID);
            Debug.Log($"[CityWallRepairPanel SERVER] WALL REPAIRED: Section {sectionToRepair} +{repairHPBonus} HP on City {targetCityID}. Consumed {requiredSteelCost} Steel.");
        }
    }
}
