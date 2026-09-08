using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Handles player transition through the city airlock into the hazardous World Map.
    /// Tracks fog exposure time when players step outside protected city/ruin thermal domes.
    /// </summary>
    public class CityAirlock : NetworkBehaviour
    {
        [Header("Airlock Configuration")]
        [SerializeField] private int cityID = 1;
        [SerializeField] private float maxSafeFogExposureSeconds = 45.0f;
        [SerializeField] private float exposureDamagePerSecond = 5.0f;

        [Header("Player Tracking")]
        private float currentExposureTimer = 0f;
        private bool isPlayerInSafeDome = true;

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                isPlayerInSafeDome = true;
                currentExposureTimer = 0f;
                Debug.Log($"[CityAirlock City:{cityID}] Player entered safe dome area.");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                isPlayerInSafeDome = false;
                Debug.LogWarning($"[CityAirlock City:{cityID}] Player EXITED safe dome into the Frozen Fog! Fog exposure timer started.");
            }
        }

        private void Update()
        {
            if (!isPlayerInSafeDome)
            {
                currentExposureTimer += Time.deltaTime;

                if (currentExposureTimer >= maxSafeFogExposureSeconds)
                {
                    Debug.LogWarning($"[CityAirlock] FOG EXPOSURE DAMAGE DEALT ({exposureDamagePerSecond} DMG/s)! Return to a thermal dome!");
                }
            }
        }

        public float ExposurePercentage => Mathf.Clamp01(currentExposureTimer / maxSafeFogExposureSeconds) * 100f;
    }
}
