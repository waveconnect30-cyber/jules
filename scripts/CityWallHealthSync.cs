using System;
using UnityEngine;
using Mirror;

namespace EcoDeLasCenizas.Networking
{
    public enum WallSection
    {
        North = 0,
        South = 1,
        East = 2,
        West = 3
    }

    [System.Serializable]
    public struct WallStatus
    {
        public WallSection section;
        public float currentHP;
        public float maxHP;

        public float HealthPercentage => maxHP > 0 ? (currentHP / maxHP) * 100f : 0f;
    }

    /// <summary>
    /// Synchronizes the HP of 4 city wall sections per cityID across all multiplayer clients in PvPvE mode.
    /// Allows repairs by same-city survivors and attacks/damage by enemy city players or AI.
    /// </summary>
    public class CityWallHealthSync : NetworkBehaviour
    {
        public static CityWallHealthSync Instance { get; private set; }

        [Header("City Faction Ownership")]
        [Tooltip("City ID that owns this wall perimeter.")]
        [SyncVar] public int cityID = 1;

        [Header("Wall Health State (Synced across network)")]
        public readonly SyncList<WallStatus> wallSections = new SyncList<WallStatus>();

        [Header("Default Max HP Configuration")]
        [SerializeField] private float northWallMaxHP = 5000f;
        [SerializeField] private float southWallMaxHP = 4000f;
        [SerializeField] private float eastWallMaxHP = 4000f;
        [SerializeField] private float westWallMaxHP = 4500f;

        // Events
        public event Action<WallSection, float, float> OnWallHealthUpdated;
        public event Action<WallSection> OnWallBreached;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }
            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            InitializeWalls();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            wallSections.Callback += OnWallSectionsListChanged;
        }

        [Server]
        private void InitializeWalls()
        {
            wallSections.Clear();
            wallSections.Add(new WallStatus { section = WallSection.North, currentHP = northWallMaxHP, maxHP = northWallMaxHP });
            wallSections.Add(new WallStatus { section = WallSection.South, currentHP = southWallMaxHP, maxHP = southWallMaxHP });
            wallSections.Add(new WallStatus { section = WallSection.East,  currentHP = eastWallMaxHP,  maxHP = eastWallMaxHP });
            wallSections.Add(new WallStatus { section = WallSection.West,  currentHP = westWallMaxHP,  maxHP = westWallMaxHP });

            Debug.Log($"[CityWallHealthSync City:{cityID}] Initialized 4 wall sections on Server.");
        }

        private void OnWallSectionsListChanged(SyncList<WallStatus>.Operation op, int index, WallStatus oldItem, WallStatus newItem)
        {
            Debug.Log($"[CityWallHealthSync City:{cityID}] Wall {newItem.section} updated: {newItem.currentHP}/{newItem.maxHP} HP ({newItem.HealthPercentage:F1}%)");
            OnWallHealthUpdated?.Invoke(newItem.section, newItem.currentHP, newItem.maxHP);

            if (newItem.currentHP <= 0f && oldItem.currentHP > 0f)
            {
                OnWallBreached?.Invoke(newItem.section);
            }
        }

        /// <summary>
        /// Applies damage to a specific wall section. Accepts damage from hostile cityIDs or AI (-1).
        /// </summary>
        [Server]
        public void DamageWall(WallSection section, float damageAmount, int attackerCityID = -1)
        {
            if (attackerCityID == cityID)
            {
                Debug.LogWarning($"[CityWallHealthSync City:{cityID}] Prevented friendly fire damage from player in City {attackerCityID}.");
                return;
            }

            int index = (int)section;
            if (index < 0 || index >= wallSections.Count) return;

            WallStatus status = wallSections[index];
            status.currentHP = Mathf.Max(0f, status.currentHP - damageAmount);
            wallSections[index] = status;

            RpcNotifyWallDamaged(section, status.currentHP, damageAmount, attackerCityID);

            if (status.currentHP <= 0f)
            {
                RpcNotifyWallBreached(section);
            }
        }

        /// <summary>
        /// Repairs a wall section. Only same-city members (matching cityID) can repair.
        /// </summary>
        [Server]
        public void RepairWall(WallSection section, float repairAmount, int repairerCityID)
        {
            if (repairerCityID != cityID)
            {
                Debug.LogWarning($"[CityWallHealthSync City:{cityID}] Rejected repair attempt by foreign CityID {repairerCityID}.");
                return;
            }

            int index = (int)section;
            if (index < 0 || index >= wallSections.Count) return;

            WallStatus status = wallSections[index];
            status.currentHP = Mathf.Min(status.maxHP, status.currentHP + repairAmount);
            wallSections[index] = status;

            RpcNotifyWallRepaired(section, status.currentHP, repairAmount);
        }

        [ClientRpc]
        private void RpcNotifyWallDamaged(WallSection section, float newHP, float damageDealt, int attackerCityID)
        {
            Debug.LogWarning($"[CityWallHealthSync RPC City:{cityID}] {section} Wall damaged by {damageDealt} from CityID {attackerCityID}. Remaining HP: {newHP}");
        }

        [ClientRpc]
        private void RpcNotifyWallRepaired(WallSection section, float newHP, float amountRepaired)
        {
            Debug.Log($"[CityWallHealthSync RPC City:{cityID}] {section} Wall repaired by {amountRepaired}. Current HP: {newHP}");
        }

        [ClientRpc]
        private void RpcNotifyWallBreached(WallSection section)
        {
            Debug.LogError($"[CityWallHealthSync CRITICAL City:{cityID}] {section} Wall HAS BEEN BREACHED!");
        }

        public WallStatus GetWallStatus(WallSection section)
        {
            int index = (int)section;
            if (index >= 0 && index < wallSections.Count)
            {
                return wallSections[index];
            }
            return default;
        }
    }
}
