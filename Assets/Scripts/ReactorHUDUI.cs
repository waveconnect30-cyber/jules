using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.UI
{
    /// <summary>
    /// Manages the in-game HUD for the Central Thermometer, Ignicita fuel level,
    /// freezing warnings (-10°C threshold), and Council voting UI triggers.
    /// Dynamically binds to the connected local player's cityID rather than hardcoded static references.
    /// </summary>
    public class ReactorHUDUI : MonoBehaviour
    {
        [Header("Temperature Display")]
        [SerializeField] private TextMeshProUGUI temperatureText;
        [SerializeField] private Slider temperatureSlider;
        [SerializeField] private Image temperatureFillImage;
        [SerializeField] private Color normalTempColor = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color freezingTempColor = Color.cyan;

        [Header("Ignicita Fuel Display")]
        [SerializeField] private TextMeshProUGUI ignicitaText;
        [SerializeField] private Slider ignicitaSlider;

        [Header("Status & Warnings")]
        [SerializeField] private GameObject freezingWarningBanner;
        [SerializeField] private TextMeshProUGUI warningBannerText;
        [SerializeField] private GameObject greenhouseStatusIndicator;
        [SerializeField] private TextMeshProUGUI greenhouseStatusText;

        [Header("Council UI Modal")]
        [SerializeField] private GameObject councilPanelModal;

        private ReactorManager boundReactor;

        private void Start()
        {
            if (councilPanelModal != null)
            {
                councilPanelModal.SetActive(false);
            }

            BindToLocalPlayerCityReactor();
        }

        private void Update()
        {
            if (boundReactor == null)
            {
                BindToLocalPlayerCityReactor();
            }
        }

        public void BindToLocalPlayerCityReactor()
        {
            var localPlayer = NetworkClient.localPlayer != null
                ? NetworkClient.localPlayer.GetComponent<PlayerController>()
                : FindObjectOfType<PlayerController>();

            if (localPlayer != null)
            {
                int localCityID = localPlayer.cityID;
                ReactorManager reactor = GameManager.Instance != null
                    ? GameManager.Instance.GetReactorForCity(localCityID)
                    : FindObjectOfType<ReactorManager>();

                if (reactor != null && reactor != boundReactor)
                {
                    boundReactor = reactor;
                    boundReactor.OnTemperatureChanged.AddListener(UpdateTemperatureUI);
                    boundReactor.OnIgnicitaChanged.AddListener(UpdateIgnicitaUI);
                    boundReactor.OnGreenhouseStatusChanged.AddListener(UpdateGreenhouseUI);
                    boundReactor.OnPlayerMovementPenaltyChanged.AddListener(UpdateWarningUI);

                    UpdateTemperatureUI(boundReactor.CurrentTemperature);
                    UpdateIgnicitaUI(boundReactor.CurrentIgnicita);
                    UpdateGreenhouseUI(boundReactor.IsGreenhouseActive);
                    UpdateWarningUI(boundReactor.IsMovementSlowed);

                    Debug.Log($"[ReactorHUDUI] Dynamically bound UI to Local Player CityID:{localCityID} Reactor.");
                }
            }
        }

        public void UpdateTemperatureUI(float currentTemp)
        {
            if (temperatureText != null)
            {
                temperatureText.text = $"{currentTemp:F1}°C";
            }

            if (temperatureSlider != null)
            {
                temperatureSlider.value = Mathf.Clamp(currentTemp, -50f, 50f);
            }

            if (temperatureFillImage != null)
            {
                temperatureFillImage.color = currentTemp <= -10f ? freezingTempColor : normalTempColor;
            }
        }

        public void UpdateIgnicitaUI(float currentIgnicita)
        {
            float maxFuel = 1000f;

            if (ignicitaText != null)
            {
                ignicitaText.text = $"Ignicita: {currentIgnicita:F0} / {maxFuel:F0}";
            }

            if (ignicitaSlider != null)
            {
                ignicitaSlider.value = currentIgnicita / maxFuel;
            }
        }

        public void UpdateGreenhouseUI(bool isActive)
        {
            if (greenhouseStatusIndicator != null)
            {
                greenhouseStatusIndicator.SetActive(true);
            }

            if (greenhouseStatusText != null)
            {
                greenhouseStatusText.text = isActive ? "Invernaderos: OPERATIVOS" : "Invernaderos: CONGELADOS (0% Prod)";
                greenhouseStatusText.color = isActive ? Color.green : Color.red;
            }
        }

        public void UpdateWarningUI(bool isFrozenPenaltyActive)
        {
            if (freezingWarningBanner != null)
            {
                freezingWarningBanner.SetActive(isFrozenPenaltyActive);
            }

            if (warningBannerText != null && isFrozenPenaltyActive)
            {
                warningBannerText.text = "¡ADVERTENCIA: -10°C ALCANZADO! Vel. de Movimiento -20% | Invernaderos Desactivados";
            }
        }

        public void ToggleCouncilUI(bool visible)
        {
            if (councilPanelModal != null)
            {
                councilPanelModal.SetActive(visible);
            }
        }
    }
}
