using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using TMPro;

namespace EcoDeLasCenizas.Core
{
    /// <summary>
    /// Manages Android runtime permissions using UnityEngine.Android.Permission.
    /// Requests INTERNET, RECORD_AUDIO (voice chat), and WRITE_EXTERNAL_STORAGE.
    /// Displays a UI alert banner if critical network/voice permissions are denied.
    /// </summary>
    public class AndroidPermissionsManager : MonoBehaviour
    {
        public static AndroidPermissionsManager Instance { get; private set; }

        [Header("Permission Warning UI")]
        [SerializeField] private GameObject permissionWarningPanel;
        [SerializeField] private TextMeshProUGUI permissionWarningText;

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
            if (permissionWarningPanel != null)
            {
                permissionWarningPanel.SetActive(false);
            }

            RequestAndroidPermissions();
        }

        public void RequestAndroidPermissions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            bool criticalPermissionDenied = false;

            // 1. Microphone / Voice Chat Permission
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }

            // 2. Storage Permission
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }

            // Verify Critical Permissions
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                criticalPermissionDenied = true;
            }

            if (criticalPermissionDenied)
            {
                ShowPermissionDeniedWarning("PERMISO DENEGADO: El chat de voz y la conexión multijugador requieren permisos activos. Por favor habilítalos en Ajustes de Android.");
            }
#else
            Debug.Log("[AndroidPermissionsManager] Non-Android build target or Editor mode. Skipping Android runtime permission prompts.");
#endif
        }

        private void ShowPermissionDeniedWarning(string message)
        {
            if (permissionWarningPanel != null)
            {
                permissionWarningPanel.SetActive(true);
            }

            if (permissionWarningText != null)
            {
                permissionWarningText.text = message;
            }

            Debug.LogWarning($"[AndroidPermissionsManager] WARNING: {message}");
        }
    }
}
