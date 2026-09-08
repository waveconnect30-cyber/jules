using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Handles city raiding and looting logic in PvPvE mode.
    /// Implements IInteractable to trigger raids via player interaction [F].
    /// Server Command verifies physical proximity and target city vulnerability before atomic transfers.
    /// </summary>
    public class CityLootManager : NetworkBehaviour, IInteractable
    {
        public static CityLootManager Instance { get; private set; }

        [Header("Looting Parameters")]
        [SerializeField] private int targetCityID = 2;
        [SerializeField] private float baseRaidPercentage = 0.25f; // Steals 25% of stored Ignicita
        [SerializeField] private float raidCooldownSeconds = 60.0f;
        [SerializeField] private float maxRaidProximityDistance = 6.0f;

        private float lastRaidTimestamp = -100f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public string GetInteractionPrompt()
        {
            return $"Presiona [F] para asaltar y saquear el almacén de Ciudad {targetCityID}";
        }

        public void Interact(PlayerController player)
        {
            if (player == null) return;
            CmdInitiateCityRaid(targetCityID);
        }

        [Command(requiresAuthority = false)]
        public void CmdInitiateCityRaid(int targetCity, NetworkConnectionToClient senderConn = null)
        {
            NetworkConnectionToClient conn = senderConn ?? connectionToClient;
            if (conn == null || conn.identity == null) return;

            PlayerController raidingPlayer = conn.identity.GetComponent<PlayerController>();
            if (raidingPlayer == null || raidingPlayer.IsDead) return;

            // SERVER DISTANCE VALIDATION
            float dist = Vector3.Distance(raidingPlayer.transform.position, transform.position);
            if (dist > maxRaidProximityDistance)
            {
                Debug.LogWarning($"[CityLootManager SERVER] RAID REJECTED: Player {raidingPlayer.name} too far ({dist:F1}m > {maxRaidProximityDistance}m).");
                return;
            }

            SharedInventorySync targetWarehouse = GameManager.Instance != null ? GameManager.Instance.GetWarehouseForCity(targetCity) : null;
            ReactorManager targetReactor = GameManager.Instance != null ? GameManager.Instance.GetReactorForCity(targetCity) : null;

            if (targetWarehouse != null)
            {
                ExecuteCityRaid(raidingPlayer, targetWarehouse, targetReactor);
            }
        }

        /// <summary>
        /// Attempts to raid a target city warehouse. Triggered on Server.
        /// </summary>
        [Server]
        public void ExecuteCityRaid(PlayerController raidingPlayer, SharedInventorySync targetWarehouse, ReactorManager targetReactor)
        {
            if (raidingPlayer == null || targetWarehouse == null) return;

            if (SeasonManager.Instance != null && !SeasonManager.Instance.IsRaidAllowed())
            {
                Debug.LogWarning("[CityLootManager SERVER] RAID BLOCKED: Raid immunity active in Season Phase 1 (Settlement).");
                return;
            }

            int attackerCityID = raidingPlayer.cityID;
            int targetCity = targetWarehouse.cityID;

            if (attackerCityID == targetCity)
            {
                Debug.LogWarning($"[CityLootManager SERVER] Player in City {attackerCityID} attempted to raid own warehouse!");
                return;
            }

            if (DiplomacyManager.Instance != null && !DiplomacyManager.Instance.IsPvPAllowed(attackerCityID, targetCity))
            {
                Debug.LogWarning($"[CityLootManager SERVER] RAID BLOCKED: Active Alliance treaty between City {attackerCityID} and City {targetCity}.");
                return;
            }

            if (Time.time < lastRaidTimestamp + raidCooldownSeconds)
            {
                Debug.LogWarning($"[CityLootManager SERVER] Raid on City {targetCity} is on cooldown.");
                return;
            }

            bool isWallBreached = false;
            CityWallHealthSync targetWall = GameManager.Instance != null ? GameManager.Instance.GetWallForCity(targetCity) : null;
            if (targetWall != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (targetWall.GetWallStatus((WallSection)i).currentHP <= 0f)
                    {
                        isWallBreached = true;
                        break;
                    }
                }
            }

            bool isReactorFrozen = (targetReactor != null && targetReactor.CurrentTemperature <= -10f);

            if (!isWallBreached && !isReactorFrozen)
            {
                Debug.LogWarning($"[CityLootManager SERVER] RAID REJECTED: City {targetCity} defenses intact.");
                return;
            }

            float actualRaidPct = isReactorFrozen ? baseRaidPercentage * 1.5f : baseRaidPercentage;
            float stolenIgnicita = targetWarehouse.RaidIgnicita(actualRaidPct);

            if (stolenIgnicita > 0f)
            {
                lastRaidTimestamp = Time.time;

                SharedInventorySync attackerWarehouse = GameManager.Instance != null ? GameManager.Instance.GetWarehouseForCity(attackerCityID) : null;
                if (attackerWarehouse != null)
                {
                    attackerWarehouse.AddIgnicitaToWarehouse(stolenIgnicita, targetCity);
                }

                RpcAnnounceRaidResult(attackerCityID, targetCity, stolenIgnicita);
            }
        }

        [ClientRpc]
        private void RpcAnnounceRaidResult(int attackerCityID, int targetCityID, float amountStolen)
        {
            Debug.LogWarning($"[CityLootManager ANNOUNCEMENT] City {attackerCityID} SUCCESSFULLY RAIDED City {targetCityID} and stole {amountStolen:F1} Ignicita!");
        }
    }
}
