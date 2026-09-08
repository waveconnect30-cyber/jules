using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Capturable neutral control point located in the wild fog area.
    /// Caches warehouse references by cityID and batches resource payouts in 1-second intervals
    /// to eliminate per-frame FindObjectsOfType overhead.
    /// </summary>
    public class TerritoryNode : NetworkBehaviour
    {
        [Header("Territory Configuration")]
        [SerializeField] private string nodeName = "Nodo Geotérmico Alpha";
        [SerializeField] private float captureTimeRequired = 10.0f; // Seconds required to capture
        [SerializeField] private float passiveIgnicitaGenerationRate = 2.0f; // Ignicita per second when controlled

        [Header("State (Synced)")]
        [SyncVar] public int controllingCityID = 0; // 0 = Neutral
        [SyncVar] public float currentCaptureProgress = 0f;
        [SyncVar] public int capturingCityID = 0;

        // Optimization: Cache warehouse instances by cityID and interval timer
        private readonly Dictionary<int, SharedInventorySync> cachedWarehouses = new Dictionary<int, SharedInventorySync>();
        private float payoutIntervalTimer = 0f;

        private void Update()
        {
            if (!isServer) return;

            // Batched passive resource payout every 1.0 second interval
            if (controllingCityID > 0)
            {
                payoutIntervalTimer += Time.deltaTime;

                if (payoutIntervalTimer >= 1.0f)
                {
                    float payoutAmount = passiveIgnicitaGenerationRate * payoutIntervalTimer;
                    payoutIntervalTimer = 0f;

                    SharedInventorySync targetWarehouse = GetCachedWarehouse(controllingCityID);
                    if (targetWarehouse != null)
                    {
                        targetWarehouse.AddIgnicitaToWarehouse(payoutAmount, controllingCityID);
                    }
                }
            }
        }

        private SharedInventorySync GetCachedWarehouse(int cID)
        {
            if (cachedWarehouses.TryGetValue(cID, out SharedInventorySync wh) && wh != null)
            {
                return wh;
            }

            SharedInventorySync[] warehouses = FindObjectsOfType<SharedInventorySync>();
            foreach (var warehouse in warehouses)
            {
                if (warehouse.cityID == cID)
                {
                    cachedWarehouses[cID] = warehouse;
                    return warehouse;
                }
            }

            return null;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isServer) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            int playerCityID = player.cityID;

            if (playerCityID == controllingCityID) return;

            if (capturingCityID != playerCityID)
            {
                capturingCityID = playerCityID;
                currentCaptureProgress = 0f;
            }

            currentCaptureProgress += Time.deltaTime;

            if (currentCaptureProgress >= captureTimeRequired)
            {
                controllingCityID = playerCityID;
                currentCaptureProgress = 0f;
                Debug.Log($"[TerritoryNode SERVER] {nodeName} CAPTURED by City {controllingCityID}!");
                RpcAnnounceNodeCaptured(nodeName, controllingCityID);
            }
        }

        [ClientRpc]
        private void RpcAnnounceNodeCaptured(string node, int cityID)
        {
            Debug.Log($"[TerritoryNode RPC] {node} is now controlled by City {cityID}. Passive Ignicita active!");
        }
    }
}
