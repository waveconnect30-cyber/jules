using UnityEngine;
using Mirror;
using System;

namespace EcoDeLasCenizas.Networking
{
    /// <summary>
    /// Synchronizes shared community warehouse inventory (Ignicita crystals, Steel, Ration filters)
    /// across all networked clients in real-time.
    /// </summary>
    public class SharedInventorySync : NetworkBehaviour
    {
        public static SharedInventorySync Instance { get; private set; }

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

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        [Server]
        public void AddIgnicitaToWarehouse(float amount)
        {
            if (amount <= 0f) return;
            totalStoredIgnicita += amount;
            Debug.Log($"[SharedInventory] +{amount} Ignicita added to Warehouse. Total: {totalStoredIgnicita}");
        }

        [Server]
        public bool ConsumeIgnicitaFromWarehouse(float amount)
        {
            if (totalStoredIgnicita >= amount)
            {
                totalStoredIgnicita -= amount;
                Debug.Log($"[SharedInventory] -{amount} Ignicita withdrawn from Warehouse. Remaining: {totalStoredIgnicita}");
                return true;
            }
            return false;
        }

        private void OnTotalIgnicitaChanged(float oldVal, float newVal)
        {
            Debug.Log($"[SharedInventory Client] Ignicita stock synced: {newVal}");
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
