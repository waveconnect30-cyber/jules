using UnityEngine;
using Mirror;
using System;

namespace EcoDeLasCenizas.Networking
{
    /// <summary>
    /// Synchronizes shared community warehouse inventory (Ignicita crystals, Steel, Ration filters)
    /// for a specific cityID across all networked clients in PvPvE mode.
    /// Instances are registered by cityID in GameManager rather than using a global static singleton.
    /// </summary>
    public class SharedInventorySync : NetworkBehaviour
    {
        [Header("City Faction Ownership")]
        [SyncVar] public int cityID = 1;

        [SyncVar(hook = nameof(OnTotalIgnicitaChanged))]
        private float totalStoredIgnicita = 0f;

        [SyncVar(hook = nameof(OnTotalSteelChanged))]
        private int totalStoredSteel = 100;

        [SyncVar(hook = nameof(OnTotalRationsChanged))]
        private int totalStoredRations = 50;

        public event Action<float> OnIgnicitaStockUpdated;
        public event Action<int> OnSteelStockUpdated;
        public event Action<int> OnRationsStockUpdated;

        public float TotalStoredIgnicita => totalStoredIgnicita;
        public int TotalStoredSteel => totalStoredSteel;
        public int TotalStoredRations => totalStoredRations;

        [Server]
        public void AddIgnicitaToWarehouse(float amount, int sourceCityID = 1)
        {
            if (amount <= 0f) return;
            totalStoredIgnicita += amount;
            Debug.Log($"[SharedInventory City:{cityID}] +{amount} Ignicita added from CityID {sourceCityID}. Total: {totalStoredIgnicita}");
        }

        [Server]
        public bool ConsumeIgnicitaFromWarehouse(float amount, int requestingCityID = 1)
        {
            if (requestingCityID != cityID)
            {
                Debug.LogWarning($"[SharedInventory City:{cityID}] Blocked withdrawal by foreign CityID {requestingCityID}.");
                return false;
            }

            if (totalStoredIgnicita >= amount)
            {
                totalStoredIgnicita -= amount;
                Debug.Log($"[SharedInventory City:{cityID}] -{amount} Ignicita withdrawn by CityID {requestingCityID}. Remaining: {totalStoredIgnicita}");
                return true;
            }
            return false;
        }

        [Server]
        public bool ConsumeSteelFromWarehouse(int amount, int requestingCityID = 1)
        {
            if (requestingCityID != cityID) return false;

            if (totalStoredSteel >= amount)
            {
                totalStoredSteel -= amount;
                return true;
            }
            return false;
        }

        [Server]
        public float RaidIgnicita(float raidPercentage)
        {
            float amountRaided = totalStoredIgnicita * Mathf.Clamp01(raidPercentage);
            totalStoredIgnicita -= amountRaided;
            Debug.LogWarning($"[SharedInventory City:{cityID}] WAREHOUSE RAIDED! Lost {amountRaided:F1} Ignicita ({raidPercentage * 100}%). Remaining: {totalStoredIgnicita:F1}");
            return amountRaided;
        }

        private void OnTotalIgnicitaChanged(float oldVal, float newVal)
        {
            Debug.Log($"[SharedInventory Client City:{cityID}] Ignicita stock synced: {newVal}");
            OnIgnicitaStockUpdated?.Invoke(newVal);
        }

        private void OnTotalSteelChanged(int oldVal, int newVal)
        {
            OnSteelStockUpdated?.Invoke(newVal);
        }

        private void OnTotalRationsChanged(int oldVal, int newVal)
        {
            OnRationsStockUpdated?.Invoke(newVal);
        }
    }
}
