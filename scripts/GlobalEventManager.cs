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
    /// </summary>
    public class GlobalEventManager : NetworkBehaviour
    {
        public static GlobalEventManager Instance { get; private set; }

        [Header("Event State")]
        [SyncVar(hook = nameof(OnCurrentEventChanged))]
        public GlobalWeatherEvent activeEvent = GlobalWeatherEvent.ClearFog;

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

            Debug.LogWarning($"[GlobalEventManager SERVER] GLOBAL CLIMATE EVENT STARTED: {weatherEvent} for {durationSeconds}s!");

            if (weatherEvent == GlobalWeatherEvent.SuperIceStorm)
            {
                ApplySuperIceStormEffects(true);
            }

            RpcAnnounceEventStart(weatherEvent.ToString(), durationSeconds);
        }

        [Server]
        public void EndGlobalEvent()
        {
            Debug.Log($"[GlobalEventManager SERVER] Global Event {activeEvent} ended. Restoring normal climate conditions.");

            if (activeEvent == GlobalWeatherEvent.SuperIceStorm)
            {
                ApplySuperIceStormEffects(false);
            }

            activeEvent = GlobalWeatherEvent.ClearFog;
            RpcAnnounceEventEnded();
        }

        private void ApplySuperIceStormEffects(bool active)
        {
            float cooldownMultiplier = active ? 2.0f : 0.5f;

            ClassAbilities[] abilities = FindObjectsOfType<ClassAbilities>();
            foreach (var ab in abilities)
            {
                if (ab != null)
                {
                    ab.ModifyCooldown(cooldownMultiplier);
                    Debug.LogWarning($"[GlobalEventManager] Player ability cooldowns modified x{cooldownMultiplier} for Super Ice Storm.");
                }
            }
        }

        private void OnCurrentEventChanged(GlobalWeatherEvent oldEvt, GlobalWeatherEvent newEvt)
        {
            Debug.Log($"[GlobalEventManager CLIENT] Climate Event changed to: {newEvt}");
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
