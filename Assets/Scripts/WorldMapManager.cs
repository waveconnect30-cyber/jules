using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Manages the World Map layout, defining the central Presidential City at (0,0,0).
    /// Runs a continuous server-side presence loop checking physical presence of clan members
    /// within the capital radius at (0,0,0) during Season Phase 3 (Presidential Siege).
    /// </summary>
    public class WorldMapManager : NetworkBehaviour, IInteractable
    {
        public static WorldMapManager Instance { get; private set; }

        [Header("Presidential City Capital Settings")]
        [SerializeField] private Vector3 presidentialCityPosition = Vector3.zero;
        [SerializeField] private float capitalSiegeRadius = 15.0f;
        [SerializeField] private float siegeDurationRequired = 300f; // 5 minutes

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

        public string GetInteractionPrompt()
        {
            return "Presiona [F] para iniciar el Asedio a la Ciudad Presidencial (0,0,0)";
        }

        public void Interact(PlayerController player)
        {
            if (player == null) return;
            CmdInitiateCapitalSiege();
        }

        [Command(requiresAuthority = false)]
        private void CmdInitiateCapitalSiege(NetworkConnectionToClient senderConn = null)
        {
            NetworkConnectionToClient conn = senderConn ?? connectionToClient;
            if (conn == null || conn.identity == null) return;

            PlayerController player = conn.identity.GetComponent<PlayerController>();
            if (player == null || player.IsDead) return;

            ProcessCapitalSiege(player.cityID, Time.deltaTime * 10f);
        }

        private void Update()
        {
            if (!isServer) return;

            // CONTINUOUS SERVER PRESENCE SIEGE LOOP
            if (SeasonManager.Instance != null && SeasonManager.Instance.currentSeasonPhase == SeasonPhase.PresidentialSiege)
            {
                Collider[] hits = Physics.OverlapSphere(presidentialCityPosition, capitalSiegeRadius);
                int dominantCityID = 0;
                int count = 0;

                foreach (var hit in hits)
                {
                    PlayerController p = hit.GetComponent<PlayerController>();
                    if (p != null && !p.IsDead)
                    {
                        dominantCityID = p.cityID;
                        count++;
                    }
                }

                if (count > 0 && dominantCityID != capitalControllingCityID)
                {
                    ProcessCapitalSiege(dominantCityID, Time.deltaTime);
                }
            }
        }

        /// <summary>
        /// Registers siege progress on the central Presidential City at (0,0,0).
        /// Only allowed during Season Phase 3 (Presidential Siege).
        /// </summary>
        [Server]
        public void ProcessCapitalSiege(int attackingCityID, float deltaTime)
        {
            if (SeasonManager.Instance != null && SeasonManager.Instance.currentSeasonPhase != SeasonPhase.PresidentialSiege)
            {
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
