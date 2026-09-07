using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EcoDeLasCenizas.UI
{
    /// <summary>
    /// Manages the in-game HUD for the Central Thermometer, Ignicita fuel level,
    /// freezing warnings (-10°C threshold), and Council voting UI triggers.
    /// </summary>
    public class ReactorHUDUI : MonoBehaviour
    {
        [Header("Temperature Display")]
        [SerializeField] private TextMeshProUGUI temperatureText;
        [SerializeField] private Slider temperatureSlider;
        [SerializeField] private Image temperatureFillImage;
        [SerializeField] private Color normalTempColor = Color.orange;
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

        private void Start()
        {
            if (EcoDeLasCenizas.Core.ReactorManager.Instance != null)
            {
                var reactor = EcoDeLasCenizas.Core.ReactorManager.Instance;

                reactor.OnTemperatureChanged.AddListener(UpdateTemperatureUI);
                reactor.OnIgnicitaChanged.AddListener(UpdateIgnicitaUI);
                reactor.OnGreenhouseStatusChanged.AddListener(UpdateGreenhouseUI);
                reactor.OnPlayerMovementPenaltyChanged.AddListener(UpdateWarningUI);

                // Initial setup
                UpdateTemperatureUI(reactor.CurrentTemperature);
                UpdateIgnicitaUI(reactor.CurrentIgnicita);
                UpdateGreenhouseUI(reactor.IsGreenhouseActive);
                UpdateWarningUI(reactor.IsMovementSlowed);
            }

            if (councilPanelModal != null)
            {
                councilPanelModal.SetActive(false);
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
            float maxFuel = EcoDeLasCenizas.Core.ReactorManager.Instance != null
                ? EcoDeLasCenizas.Core.ReactorManager.Instance.MaxIgnicita
                : 1000f;

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
