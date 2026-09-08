using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Handles city raiding and looting logic in PvPvE mode.
    /// When an enemy city's reactor or wall HP reaches 0, hostile players can trigger a raid
    /// to siphon a percentage of Ignicita from the target warehouse into their own city warehouse.
    /// </summary>
    public class CityLootManager : NetworkBehaviour
    {
        public static CityLootManager Instance { get; private set; }

        [Header("Looting Parameters")]
        [SerializeField] private float baseRaidPercentage = 0.25f; // Steals 25% of stored Ignicita
        [SerializeField] private float raidCooldownSeconds = 60.0f;

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

        /// <summary>
        /// Attempts to raid a target city warehouse. Triggered by a player.
        /// </summary>
        [Server]
        public void ExecuteCityRaid(PlayerController raidingPlayer, SharedInventorySync targetWarehouse, ReactorManager targetReactor)
        {
            if (raidingPlayer == null || targetWarehouse == null) return;

            int attackerCityID = raidingPlayer.cityID;
            int targetCityID = targetWarehouse.cityID;

            if (attackerCityID == targetCityID)
            {
                Debug.LogWarning($"[CityLootManager] Player in City {attackerCityID} attempted to raid their own city warehouse!");
                return;
            }

            if (Time.time < lastRaidTimestamp + raidCooldownSeconds)
            {
                Debug.LogWarning($"[CityLootManager] Raid on City {targetCityID} is on cooldown.");
                return;
            }

            // Check if city is vulnerable (reactor out of fuel or temp critically low)
            bool isVulnerable = (targetReactor != null && targetReactor.CurrentTemperature <= -10f);

            float actualRaidPct = isVulnerable ? baseRaidPercentage * 1.5f : baseRaidPercentage;
            float stolenIgnicita = targetWarehouse.RaidIgnicita(actualRaidPct);

            if (stolenIgnicita > 0f)
            {
                lastRaidTimestamp = Time.time;

                // Find attacker's warehouse and credit the stolen Ignicita
                SharedInventorySync[] warehouses = FindObjectsOfType<SharedInventorySync>();
                foreach (var wh in warehouses)
                {
                    if (wh.cityID == attackerCityID)
                    {
                        wh.AddIgnicitaToWarehouse(stolenIgnicita, targetCityID);
                        break;
                    }
                }

                RpcAnnounceRaidResult(attackerCityID, targetCityID, stolenIgnicita);
            }
        }

        [ClientRpc]
        private void RpcAnnounceRaidResult(int attackerCityID, int targetCityID, float amountStolen)
        {
            Debug.LogWarning($"[CityLootManager ANNOUNCEMENT] City {attackerCityID} SUCCESSFULLY RAIDED City {targetCityID} and stole {amountStolen:F1} Ignicita!");
        }
    }
}
