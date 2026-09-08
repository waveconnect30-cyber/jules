using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.UI
{
    /// <summary>
    /// Controls screen vignette/frost border overlays and animated text alerts when city temperature drops below -10°C.
    /// Isolated to client: vignetting activates ONLY if the freezing city matches local player's cityID.
    /// </summary>
    public class ScreenFrostPostProcessUI : MonoBehaviour
    {
        [Header("Frost Overlay UI Elements")]
        [SerializeField] private Image frostBorderOverlayImage;
        [SerializeField] private GameObject warningAlertBanner;
        [SerializeField] private TextMeshProUGUI warningAlertText;

        [Header("Frost Effect Settings")]
        [SerializeField] private float maxFrostAlpha = 0.85f;
        [SerializeField] private float pulseSpeed = 2.0f;

        private bool isFreezingActive = false;

        private void Start()
        {
            if (frostBorderOverlayImage != null)
            {
                Color c = frostBorderOverlayImage.color;
                c.a = 0f;
                frostBorderOverlayImage.color = c;
            }

            if (warningAlertBanner != null)
            {
                warningAlertBanner.SetActive(false);
            }
        }

        private void Update()
        {
            if (isFreezingActive && frostBorderOverlayImage != null)
            {
                float alpha = (Mathf.Sin(Time.time * pulseSpeed) * 0.15f) + (maxFrostAlpha - 0.15f);
                Color c = frostBorderOverlayImage.color;
                c.a = Mathf.Clamp01(alpha);
                frostBorderOverlayImage.color = c;
            }
        }

        /// <summary>
        /// Evaluates temperature drop against local player's cityID.
        /// Ignores freezing alerts from foreign enemy cities.
        /// </summary>
        public void EvaluateCityTemperature(int reactorCityID, float temperature)
        {
            var localPlayer = NetworkClient.localPlayer != null
                ? NetworkClient.localPlayer.GetComponent<PlayerController>()
                : FindObjectOfType<PlayerController>();

            if (localPlayer != null && localPlayer.cityID != reactorCityID)
            {
                // Ignore temperature changes from enemy city reactors
                return;
            }

            bool shouldAlert = temperature <= -10.0f;
            SetFreezingAlertState(shouldAlert);
        }

        public void SetFreezingAlertState(bool active)
        {
            isFreezingActive = active;

            if (warningAlertBanner != null)
            {
                warningAlertBanner.SetActive(active);
            }

            if (warningAlertText != null && active)
            {
                warningAlertText.text = "¡TEMPERATURA CRÍTICA DE TU CIUDAD: -10°C! ESCUADRÓN AFECTADO POR CONGELAMIENTO (-20% VELOCIDAD)";
            }

            if (!active && frostBorderOverlayImage != null)
            {
                Color c = frostBorderOverlayImage.color;
                c.a = 0f;
                frostBorderOverlayImage.color = c;
            }
        }
    }
}
