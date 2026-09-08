using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EcoDeLasCenizas.UI
{
    /// <summary>
    /// Controls screen vignette/frost border overlays and animated text alerts when city temperature drops below -10°C.
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

            if (EcoDeLasCenizas.Core.ReactorManager.Instance != null)
            {
                EcoDeLasCenizas.Core.ReactorManager.Instance.OnPlayerMovementPenaltyChanged.AddListener(SetFreezingAlertState);
                SetFreezingAlertState(EcoDeLasCenizas.Core.ReactorManager.Instance.IsMovementSlowed);
            }
        }

        private void Update()
        {
            if (isFreezingActive && frostBorderOverlayImage != null)
            {
                // Pulsing frost vignette effect
                float alpha = (Mathf.Sin(Time.time * pulseSpeed) * 0.15f) + (maxFrostAlpha - 0.15f);
                Color c = frostBorderOverlayImage.color;
                c.a = Mathf.Clamp01(alpha);
                frostBorderOverlayImage.color = c;
            }
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
                warningAlertText.text = "¡TEMPERATURA CRÍTICA: -10°C! ESCUADRÓN AFECTADO POR CONGELAMIENTO (-20% VELOCIDAD)";
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
