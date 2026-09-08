using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Player;

namespace EcoDeLasCenizas.Gameplay
{
    public enum GlobalWeatherEvent
    {
        ClearFog,
        SuperIceStorm,   // Accelerates temperature decay to -10°C/min
        ToxicMistSurge,  // Increases filter consumption
        SolarGeyser      // Boosts Ignicita node yields
    }

    /// <summary>
    /// Manages server-wide climate events, such as the 'Super Ice Storm' which accelerates
    /// temperature decay across all city reactors to -10°C/min and doubles ability cooldowns.
    /// Uses SyncVars to notify cooldown multipliers and temperature decay rates without stacking multipliers.
    /// </summary>
    public class GlobalEventManager : NetworkBehaviour
    {
        public static GlobalEventManager Instance { get; private set; }

        [Header("Event State")]
        [SyncVar(hook = nameof(OnCurrentEventChanged))]
        public GlobalWeatherEvent activeEvent = GlobalWeatherEvent.ClearFog;

        [SyncVar(hook = nameof(OnCooldownMultiplierChanged))]
        public float currentCooldownMultiplier = 1.0f;

        [SyncVar] public float eventTimeRemaining = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!isServer) return;

            if (eventTimeRemaining > 0f)
            {
                eventTimeRemaining -= Time.deltaTime;
                if (eventTimeRemaining <= 0f)
                {
                    EndGlobalEvent();
                }
            }
        }

        /// <summary>
        /// Triggers a global weather event across the server.
        /// </summary>
        [Server]
        public void TriggerGlobalEvent(GlobalWeatherEvent weatherEvent, float durationSeconds = 120f)
        {
            activeEvent = weatherEvent;
            eventTimeRemaining = durationSeconds;

            if (weatherEvent == GlobalWeatherEvent.SuperIceStorm)
            {
                currentCooldownMultiplier = 2.0f;
            }
            else
            {
                currentCooldownMultiplier = 1.0f;
            }

            Debug.LogWarning($"[GlobalEventManager SERVER] GLOBAL CLIMATE EVENT STARTED: {weatherEvent} for {durationSeconds}s! Cooldown Mult: x{currentCooldownMultiplier}");
            RpcAnnounceEventStart(weatherEvent.ToString(), durationSeconds);
        }

        [Server]
        public void EndGlobalEvent()
        {
            Debug.Log($"[GlobalEventManager SERVER] Global Event {activeEvent} ended. Restoring normal climate conditions.");

            activeEvent = GlobalWeatherEvent.ClearFog;
            currentCooldownMultiplier = 1.0f;

            RpcAnnounceEventEnded();
        }

        private void OnCooldownMultiplierChanged(float oldMult, float newMult)
        {
            Debug.Log($"[GlobalEventManager SyncVar] Cooldown multiplier updated on client: x{newMult}");
        }

        private void OnCurrentEventChanged(GlobalWeatherEvent oldEvt, GlobalWeatherEvent newEvt)
        {
            Debug.Log($"[GlobalEventManager CLIENT SyncVar] Climate Event updated: {newEvt}");
        }

        [ClientRpc]
        private void RpcAnnounceEventStart(string eventName, float duration)
        {
            Debug.LogError($"[GLOBAL CLIMATE ALERT] WARNING: {eventName} HAS BEGUN! Duration: {duration}s. Extreme freezing and 2x ability cooldowns in effect!");
        }

        [ClientRpc]
        private void RpcAnnounceEventEnded()
        {
            Debug.Log("[GLOBAL CLIMATE ALERT] The severe climate storm has subsided.");
        }
    }
}
