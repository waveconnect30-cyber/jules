using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    /// <summary>
    /// Handles player transition through the city airlock into the hazardous World Map.
    /// Tracks per-player fog exposure time when players step outside protected city/ruin thermal domes,
    /// deducting actual player HP when exposure exceeds safe limits.
    /// </summary>
    public class CityAirlock : NetworkBehaviour
    {
        [Header("Airlock Configuration")]
        [SerializeField] private int cityID = 1;
        [SerializeField] private float maxSafeFogExposureSeconds = 45.0f;
        [SerializeField] private float exposureDamagePerSecond = 5.0f;

        // Per-player fog exposure tracking
        private readonly Dictionary<PlayerController, float> playerExposureTimers = new Dictionary<PlayerController, float>();
        private readonly HashSet<PlayerController> playersInDome = new HashSet<PlayerController>();

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                playersInDome.Add(player);
                playerExposureTimers[player] = 0f;
                Debug.Log($"[CityAirlock City:{cityID}] Player {player.name} (CityID:{player.cityID}) entered safe dome area.");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                playersInDome.Remove(player);
                if (!playerExposureTimers.ContainsKey(player))
                {
                    playerExposureTimers[player] = 0f;
                }
                Debug.LogWarning($"[CityAirlock City:{cityID}] Player {player.name} EXITED safe dome into Frozen Fog! Exposure timer started.");
            }
        }

        private void Update()
        {
            if (!isServer) return;

            List<PlayerController> trackedPlayers = new List<PlayerController>(playerExposureTimers.Keys);

            foreach (var player in trackedPlayers)
            {
                if (player == null)
                {
                    playerExposureTimers.Remove(player);
                    continue;
                }

                if (!playersInDome.Contains(player))
                {
                    playerExposureTimers[player] += Time.deltaTime;

                    if (playerExposureTimers[player] >= maxSafeFogExposureSeconds)
                    {
                        float damageThisFrame = exposureDamagePerSecond * Time.deltaTime;
                        player.TakeDamage(damageThisFrame);
                        Debug.LogWarning($"[CityAirlock] FOG DAMAGE DEALT ({damageThisFrame:F1} HP) to {player.name}! Remaining HP: {player.CurrentHP}/{player.MaxHP}");
                    }
                }
            }
        }

        public float GetPlayerExposurePercentage(PlayerController player)
        {
            if (player != null && playerExposureTimers.TryGetValue(player, out float timer))
            {
                return Mathf.Clamp01(timer / maxSafeFogExposureSeconds) * 100f;
            }
            return 0f;
        }
    }
}
