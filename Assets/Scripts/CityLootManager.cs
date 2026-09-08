using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Handles city raiding and looting logic in PvPvE mode.
    /// Requires target city wall HP == 0 OR critical reactor freezing before allowing raids.
    /// Checks DiplomacyManager to prohibit raids between allied cities.
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
        /// Enforces Alliance treaty checks and vulnerability conditions (wall breached or reactor frozen).
        /// </summary>
        [Server]
        public void ExecuteCityRaid(PlayerController raidingPlayer, SharedInventorySync targetWarehouse, ReactorManager targetReactor)
        {
            if (raidingPlayer == null || targetWarehouse == null) return;

            // Enforce Season Phase Raid Immunity (Phase 1 Settlement blocks raiding)
            if (SeasonManager.Instance != null && !SeasonManager.Instance.IsRaidAllowed())
            {
                Debug.LogWarning("[CityLootManager SERVER] RAID BLOCKED: Raid immunity is active during Season Phase 1 (Settlement).");
                return;
            }

            int attackerCityID = raidingPlayer.cityID;
            int targetCityID = targetWarehouse.cityID;

            if (attackerCityID == targetCityID)
            {
                Debug.LogWarning($"[CityLootManager SERVER] Player in City {attackerCityID} attempted to raid their own city warehouse!");
                return;
            }

            // Diplomacy Treaty Check: Block raids between Allied cities
            if (DiplomacyManager.Instance != null && !DiplomacyManager.Instance.IsPvPAllowed(attackerCityID, targetCityID))
            {
                Debug.LogWarning($"[CityLootManager SERVER] RAID BLOCKED: City {attackerCityID} and City {targetCityID} maintain an active Alliance treaty.");
                return;
            }

            if (Time.time < lastRaidTimestamp + raidCooldownSeconds)
            {
                Debug.LogWarning($"[CityLootManager SERVER] Raid on City {targetCityID} is on cooldown.");
                return;
            }

            // Vulnerability Check: Target city must have at least 1 wall breached OR reactor frozen
            bool isWallBreached = false;
            CityWallHealthSync targetWall = FindCityWall(targetCityID);
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
                Debug.LogWarning($"[CityLootManager SERVER] RAID REJECTED: City {targetCityID} is fully defended (walls intact and reactor operational).");
                return;
            }

            float actualRaidPct = isReactorFrozen ? baseRaidPercentage * 1.5f : baseRaidPercentage;
            float stolenIgnicita = targetWarehouse.RaidIgnicita(actualRaidPct);

            if (stolenIgnicita > 0f)
            {
                lastRaidTimestamp = Time.time;

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

        private CityWallHealthSync FindCityWall(int cID)
        {
            CityWallHealthSync[] walls = FindObjectsOfType<CityWallHealthSync>();
            foreach (var w in walls)
            {
                if (w.cityID == cID) return w;
            }
            return null;
        }

        [ClientRpc]
        private void RpcAnnounceRaidResult(int attackerCityID, int targetCityID, float amountStolen)
        {
            Debug.LogWarning($"[CityLootManager ANNOUNCEMENT] City {attackerCityID} SUCCESSFULLY RAIDED City {targetCityID} and stole {amountStolen:F1} Ignicita!");
        }
    }
}
