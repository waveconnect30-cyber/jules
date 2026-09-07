using System;
using UnityEngine;
using UnityEngine.Events;

namespace EcoDeLasCenizas.Core
{
    /// <summary>
    /// Manages the Central Geothermal Reactor, Ignicita fuel consumption, city temperature decay,
    /// and global penalties for freezing conditions (-10°C threshold).
    /// </summary>
    public class ReactorManager : MonoBehaviour
    {
        public static ReactorManager Instance { get; private set; }

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
        public float CurrentIgnicita => currentIgnicita;
        public float MaxIgnicita => maxIgnicita;
        public float CurrentTemperature => currentTemperature;
        public bool IsGreenhouseActive => isGreenhouseActive;
        public bool IsMovementSlowed => isMovementSlowed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
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

        /// <summary>
        /// Updates fuel consumption and temperature decay every frame.
        /// </summary>

        public void ProcessReactorCycle(float deltaTime)
        {
            if (currentIgnicita > 0f)
            {
                // Consume Ignicita every second
                currentIgnicita -= ignicitaConsumptionRate * deltaTime;
                if (currentIgnicita < 0f)
                {
                    currentIgnicita = 0f;
                }
                OnIgnicitaChanged?.Invoke(currentIgnicita);
            }
            else
            {
                // When Ignicita reaches 0, temperature drops by -5°C per minute (-5 / 60 per second)
                float tempLossThisFrame = (temperatureDecayRatePerMinute / 60.0f) * deltaTime;
                currentTemperature -= tempLossThisFrame;
                OnTemperatureChanged?.Invoke(currentTemperature);

                EvaluateTemperatureEffects();
            }
        }

        /// <summary>
        /// Evaluates current temperature against critical thresholds (-10°C).
        /// </summary>
        private void EvaluateTemperatureEffects()
        {
            bool shouldBeFrozen = currentTemperature <= criticalTemperatureThreshold;

            if (shouldBeFrozen)
            {
                if (isGreenhouseActive)
                {
                    isGreenhouseActive = false;
                    Debug.LogWarning("[ReactorManager] CRITICAL WARNING: Temperature dropped below -10°C! Greenhouses DISABLED.");
                    OnGreenhouseStatusChanged?.Invoke(false);
                }

                if (!isMovementSlowed)
                {
                    isMovementSlowed = true;
                    Debug.LogWarning("[ReactorManager] CRITICAL WARNING: Freezing weather reduces player movement speed by 20%.");
                    OnPlayerMovementPenaltyChanged?.Invoke(true);
                }
            }
            else
            {
                if (!isGreenhouseActive)
                {
                    isGreenhouseActive = true;
                    Debug.Log("[ReactorManager] Temperature restored above -10°C. Greenhouses RE-ENABLED.");
                    OnGreenhouseStatusChanged?.Invoke(true);
                }

                if (isMovementSlowed)
                {
                    isMovementSlowed = false;
                    Debug.Log("[ReactorManager] Temperature restored. Player movement speed NORMALIZED.");
                    OnPlayerMovementPenaltyChanged?.Invoke(false);
                }
            }

            if (currentTemperature <= -50.0f)
            {
                OnCityFrozenSolid?.Invoke();
            }
        }

        /// <summary>
        /// Allows players to deposit Ignicita into the global shared container.
        /// Increases fuel reserve and raises city temperature.
        /// </summary>
        /// <param name="amount">Amount of Ignicita deposited.</param>
        public void DepositIgnicita(float amount)
        {
            if (amount <= 0f) return;

            currentIgnicita = Mathf.Min(currentIgnicita + amount, maxIgnicita);

            // Warm up city upon adding Ignicita fuel
            float tempIncrease = amount * temperatureGainPerIgnicita;
            currentTemperature = Mathf.Min(currentTemperature + tempIncrease, maxTemperature);

            Debug.Log($"[ReactorManager] Deposited {amount} Ignicita. New Total Fuel: {currentIgnicita:F1}, New Temp: {currentTemperature:F1}°C");

            OnIgnicitaChanged?.Invoke(currentIgnicita);
            OnTemperatureChanged?.Invoke(currentTemperature);

            EvaluateTemperatureEffects();
        }
    }
}
