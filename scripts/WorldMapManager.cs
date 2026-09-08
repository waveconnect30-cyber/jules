using UnityEngine;
using Mirror;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Manages the World Map layout, defining the central Presidential City at (0,0,0)
    /// and siege capture events for controlling the central megacity capital.
    /// Unlocked exclusively during Season Phase 3 (Presidential Siege).
    /// </summary>
    public class WorldMapManager : NetworkBehaviour
    {
        public static WorldMapManager Instance { get; private set; }

        [Header("Presidential City Capital Settings")]
        [SerializeField] private Vector3 presidentialCityPosition = Vector3.zero;
        [SerializeField] private float siegeDurationRequired = 300f; // 5 minutes to capture capital

        [Header("Capital City State (Synced)")]
        [SyncVar] public int capitalControllingCityID = 0; // 0 = Unclaimed
        [SyncVar] public float capitalSiegeTimer = 0f;
        [SyncVar] public int siegeAttackingCityID = 0;

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
        /// Registers siege progress on the central Presidential City at (0,0,0).
        /// Only allowed during Season Phase 3 (Presidential Siege).
        /// </summary>
        [Server]
        public void ProcessCapitalSiege(int attackingCityID, float deltaTime)
        {
            // Verify Season Phase 3 (PresidentialSiege)
            if (SeasonManager.Instance != null && SeasonManager.Instance.currentSeasonPhase != SeasonPhase.PresidentialSiege)
            {
                Debug.LogWarning("[WorldMapManager] CAPITAL SIEGE BLOCKED: Presidential City capture is only unlocked during Season Phase 3 (Presidential Siege).");
                return;
            }

            if (attackingCityID == capitalControllingCityID) return;

            if (siegeAttackingCityID != attackingCityID)
            {
                siegeAttackingCityID = attackingCityID;
                capitalSiegeTimer = 0f;
            }

            capitalSiegeTimer += deltaTime;

            if (capitalSiegeTimer >= siegeDurationRequired)
            {
                capitalControllingCityID = attackingCityID;
                capitalSiegeTimer = 0f;
                Debug.LogWarning($"[WorldMapManager SERVER] PRESIDENTIAL CITY CAPITAL CAPTURED BY CITY {capitalControllingCityID}!");
                RpcAnnounceCapitalCaptured(capitalControllingCityID);
            }
        }

        [ClientRpc]
        private void RpcAnnounceCapitalCaptured(int cityID)
        {
            Debug.LogError($"[WORLD MAP CAPITAL ALERT] CITY {cityID} HAS CAPTURED THE PRESIDENTIAL CITY AT (0,0,0)! ALL CLAN BUFFS DOUBLED!");
        }
    }
}
