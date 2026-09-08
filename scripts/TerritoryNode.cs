using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Capturable neutral control point located in the wild fog area.
    /// When a player clan holds the area for X seconds, it generates a passive stream of Ignicita
    /// directly into that clan's city warehouse.
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

        private void Update()
        {
            if (!isServer) return;

            // Passive resource generation for controlling city
            if (controllingCityID > 0)
            {
                float generatedAmount = passiveIgnicitaGenerationRate * Time.deltaTime;

                SharedInventorySync[] warehouses = FindObjectsOfType<SharedInventorySync>();
                foreach (var warehouse in warehouses)
                {
                    if (warehouse.cityID == controllingCityID)
                    {
                        warehouse.AddIgnicitaToWarehouse(generatedAmount, controllingCityID);
                        break;
                    }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isServer) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            int playerCityID = player.cityID;

            // If node already owned by player's city, nothing to capture
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
                Debug.Log($"[TerritoryNode] {nodeName} CAPTURED by City {controllingCityID}!");
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
