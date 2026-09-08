using UnityEngine;

namespace EcoDeLasCenizas.Core
{
    /// <summary>
    /// Manages adaptive graphic quality settings across PC and Mobile (Android) platforms.
    /// Reduces render distances, shadow cascades, and particle counts on mobile devices
    /// to guarantee stable 30-60 FPS performance during Cross-Play sessions.
    /// </summary>
    public class QualitySettingsManager : MonoBehaviour
    {
        public static QualitySettingsManager Instance { get; private set; }

        [Header("Target Framerate Settings")]
        [SerializeField] private int pcTargetFramerate = 60;
        [SerializeField] private int mobileTargetFramerate = 30;

        [Header("Mobile Optimization Overrides")]
        [SerializeField] private float mobileCameraRenderDistance = 150f;
        [SerializeField] private float pcCameraRenderDistance = 500f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ApplyPlatformQualityProfile();
        }

        public void ApplyPlatformQualityProfile()
        {
            bool isMobile = Application.isMobilePlatform || Application.platform == RuntimePlatform.Android;

            if (isMobile)
            {
                Application.targetFrameRate = mobileTargetFramerate;
                QualitySettings.vSyncCount = 0;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowDistance = 35.0f;
                QualitySettings.masterTextureLimit = 1; // Half resolution textures on mobile
                QualitySettings.particleRaycastBlendDistance = 10f;

                if (Camera.main != null)
                {
                    Camera.main.farClipPlane = mobileCameraRenderDistance;
                }

                Debug.LogWarning($"[QualitySettingsManager] MOBILE PROFILE APPLIED: Target FPS={mobileTargetFramerate}, FarClip={mobileCameraRenderDistance}m, Shadows=HardOnly");
            }
            else
            {
                Application.targetFrameRate = pcTargetFramerate;
                QualitySettings.vSyncCount = 1;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowDistance = 150.0f;
                QualitySettings.masterTextureLimit = 0; // Full resolution textures on PC

                if (Camera.main != null)
                {
                    Camera.main.farClipPlane = pcCameraRenderDistance;
                }

                Debug.Log($"[QualitySettingsManager] PC HIGH PROFILE APPLIED: Target FPS={pcTargetFramerate}, FarClip={pcCameraRenderDistance}m, Shadows=All");
            }
        }
    }
}
