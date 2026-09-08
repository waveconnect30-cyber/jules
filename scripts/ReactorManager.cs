using System;
using UnityEngine;
using UnityEngine.Events;

namespace EcoDeLasCenizas.Core
{
    /// <summary>
    /// Manages the Central Geothermal Reactor, Ignicita fuel consumption, city temperature decay,
    /// cityID ownership in PvPvE mode, and global penalties for freezing conditions (-10°C threshold).
    /// </summary>
    public class ReactorManager : MonoBehaviour
    {
        public static ReactorManager Instance { get; private set; }

        [Header("City Faction Ownership")]
        [Tooltip("The City / Clan ID that owns this reactor.")]
        [SerializeField] private int cityID = 1;

        [Header("Reactor Fuel Settings")]
        [Tooltip("Current amount of Ignicita fuel stored in the central reactor container.")]
        [SerializeField] private float currentIgnicita = 100f;

        [Tooltip("Maximum capacity for Ignicita in the reactor container.")]
        [SerializeField] private float maxIgnicita = 1000f;

        [Tooltip("Amount of Ignicita consumed per second under normal operation.")]
        [SerializeField] private float ignicitaConsumptionRate = 1.0f;

        [Tooltip("Temperature gain per unit of Ignicita deposited.")]
        [SerializeField] private float temperatureGainPerIgnicita = 2.0f;

        [Header("Temperature Settings")]
        [Tooltip("Current temperature of the city in Celsius.")]
        [SerializeField] private float currentTemperature = 20.0f;

        [Tooltip("Maximum allowed city temperature in Celsius.")]
        [SerializeField] private float maxTemperature = 50.0f;

        [Tooltip("Temperature loss rate in degrees Celsius per minute when Ignicita is at 0.")]
        [SerializeField] private float temperatureDecayRatePerMinute = 5.0f;

        [Tooltip("Critical temperature threshold below which greenhouses disable and movement slows.")]
        [SerializeField] private float criticalTemperatureThreshold = -10.0f;

        [Header("Status Flags")]
        private bool isGreenhouseActive = true;
        private bool isMovementSlowed = false;

        [Header("Events")]
        public UnityEvent<float> OnIgnicitaChanged;
        public UnityEvent<float> OnTemperatureChanged;
        public UnityEvent<bool> OnGreenhouseStatusChanged;
        public UnityEvent<bool> OnPlayerMovementPenaltyChanged;
        public UnityEvent OnCityFrozenSolid;

        // Public Read-only Properties
        public int CityID => cityID;
        public float CurrentIgnicita => currentIgnicita;
        public float MaxIgnicita => maxIgnicita;
        public float CurrentTemperature => currentTemperature;
        public bool IsGreenhouseActive => isGreenhouseActive;
        public bool IsMovementSlowed => isMovementSlowed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // In multi-city mode, multiple reactor instances exist per cityID
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            OnIgnicitaChanged?.Invoke(currentIgnicita);
            OnTemperatureChanged?.Invoke(currentTemperature);
            EvaluateTemperatureEffects();
        }

        private void Update()
        {
            ProcessReactorCycle(Time.deltaTime);
        }

        public void ProcessReactorCycle(float deltaTime)
        {
            if (currentIgnicita > 0f)
            {
                currentIgnicita -= ignicitaConsumptionRate * deltaTime;
                if (currentIgnicita < 0f)
                {
                    currentIgnicita = 0f;
                }
                OnIgnicitaChanged?.Invoke(currentIgnicita);
            }
            else
            {
                float tempLossThisFrame = (temperatureDecayRatePerMinute / 60.0f) * deltaTime;
                currentTemperature -= tempLossThisFrame;
                OnTemperatureChanged?.Invoke(currentTemperature);

                EvaluateTemperatureEffects();
            }
        }

        private void EvaluateTemperatureEffects()
        {
            bool shouldBeFrozen = currentTemperature <= criticalTemperatureThreshold;

            if (shouldBeFrozen)
            {
                if (isGreenhouseActive)
                {
                    isGreenhouseActive = false;
                    Debug.LogWarning($"[ReactorManager City:{cityID}] CRITICAL WARNING: Temperature dropped below -10°C! Greenhouses DISABLED.");
                    OnGreenhouseStatusChanged?.Invoke(false);
                }

                if (!isMovementSlowed)
                {
                    isMovementSlowed = true;
                    Debug.LogWarning($"[ReactorManager City:{cityID}] CRITICAL WARNING: Freezing weather reduces player movement speed by 20%.");
                    OnPlayerMovementPenaltyChanged?.Invoke(true);
                }
            }
            else
            {
                if (!isGreenhouseActive)
                {
                    isGreenhouseActive = true;
                    Debug.Log($"[ReactorManager City:{cityID}] Temperature restored above -10°C. Greenhouses RE-ENABLED.");
                    OnGreenhouseStatusChanged?.Invoke(true);
                }

                if (isMovementSlowed)
                {
                    isMovementSlowed = false;
                    Debug.Log($"[ReactorManager City:{cityID}] Temperature restored. Player movement speed NORMALIZED.");
                    OnPlayerMovementPenaltyChanged?.Invoke(false);
                }
            }

            if (currentTemperature <= -50.0f)
            {
                OnCityFrozenSolid?.Invoke();
            }
        }

        /// <summary>
        /// Deposit Ignicita fuel into reactor. Accepts deposits only from matching cityID survivors.
        /// </summary>
        public void DepositIgnicita(float amount, int depositorCityID)
        {
            if (depositorCityID != cityID)
            {
                Debug.LogWarning($"[ReactorManager City:{cityID}] Rejected deposit from enemy player (CityID:{depositorCityID}).");
                return;
            }

            DepositIgnicita(amount);
        }

        public void DepositIgnicita(float amount)
        {
            if (amount <= 0f) return;

            currentIgnicita = Mathf.Min(currentIgnicita + amount, maxIgnicita);

            float tempIncrease = amount * temperatureGainPerIgnicita;
            currentTemperature = Mathf.Min(currentTemperature + tempIncrease, maxTemperature);

            Debug.Log($"[ReactorManager City:{cityID}] Deposited {amount} Ignicita. New Fuel: {currentIgnicita:F1}, New Temp: {currentTemperature:F1}°C");

            OnIgnicitaChanged?.Invoke(currentIgnicita);
            OnTemperatureChanged?.Invoke(currentTemperature);

            EvaluateTemperatureEffects();
        }
    }
}
