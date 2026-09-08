using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.Networking;

namespace EcoDeLasCenizas.Gameplay
{
    public enum GamePhase
    {
        ExpeditionPhase,   // Exploration 3D & Gathering Ignicita
        SiegePhase,        // Real-time wall defense against Sombras Heladas
        CouncilPhase,      // Democratic resource voting
        GameOver           // Shared Defeat condition triggered
    }

    /// <summary>
    /// Core Game Manager orchestrating phase cycles and multi-city instance registry by cityID.
    /// Manages lookups for Reactors, Walls, and Warehouses per cityID to isolate independent city states.
    /// Tracks per-city defeat status without ending the match for unaffected cities.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Phase Management")]
        [SyncVar(hook = nameof(OnPhaseSyncHook))] [SerializeField] private GamePhase currentPhase = GamePhase.ExpeditionPhase;
        [SerializeField] private float expeditionPhaseDuration = 180f; // 3 minutes
        [SerializeField] private float siegePhaseDuration = 120f;      // 2 minutes
        [SyncVar] private float phaseTimer = 0f;

        [Header("Multi-City Systems Registry")]
        private readonly Dictionary<int, ReactorManager> registeredReactors = new Dictionary<int, ReactorManager>();
        private readonly Dictionary<int, CityWallHealthSync> registeredWalls = new Dictionary<int, CityWallHealthSync>();
        private readonly Dictionary<int, SharedInventorySync> registeredWarehouses = new Dictionary<int, SharedInventorySync>();

        /// <summary>
        /// Local server-side HashSet tracking defeated city IDs.
        /// NOTE: Currently records and announces per-city defeat events via RpcNotifyCityDefeated.
        /// Full action blocking and respawn locking for defeated city members will be connected
        /// during local testing when player input/respawn gates are linked.
        /// </summary>
        [Header("Isolated City Defeat Tracking")]
        private readonly HashSet<int> defeatedCityIDs = new HashSet<int>();
        private readonly Dictionary<int, UnityAction> reactorFreezeHandlers = new Dictionary<int, UnityAction>();

        [Header("Events")]
        public UnityEvent<GamePhase> OnPhaseChanged;
        public UnityEvent OnTeamVictory;
        public UnityEvent<string> OnTeamDefeat;
        public UnityEvent<int, string> OnCityDefeated;

        public GamePhase CurrentPhase => currentPhase;
        public float PhaseTimerRemaining => phaseTimer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            TransitionToPhase(GamePhase.ExpeditionPhase);
            RebuildMultiCityRegistries();
        }

        /// <summary>
        /// Registers multi-city system lookups in scene and manages explicit server event listener delegates per cityID.
        /// </summary>
        public void RebuildMultiCityRegistries()
        {
            // Unsubscribe existing managed delegates to prevent duplicate callbacks
            foreach (var kvp in registeredReactors)
            {
                int cID = kvp.Key;
                ReactorManager r = kvp.Value;
                if (r != null && reactorFreezeHandlers.TryGetValue(cID, out UnityAction action))
                {
                    r.OnCityFrozenSolid.RemoveListener(action);
                }
            }
            reactorFreezeHandlers.Clear();
            registeredReactors.Clear();
            registeredWalls.Clear();
            registeredWarehouses.Clear();

            ReactorManager[] reactors = FindObjectsOfType<ReactorManager>();
            foreach (var r in reactors)
            {
                registeredReactors[r.CityID] = r;

                int cID = r.CityID;
                UnityAction handler = () => OnServerCityReactorFrozen(cID);
                reactorFreezeHandlers[cID] = handler;
                r.OnCityFrozenSolid.AddListener(handler);
            }

            CityWallHealthSync[] walls = FindObjectsOfType<CityWallHealthSync>();
            foreach (var w in walls)
            {
                registeredWalls[w.cityID] = w;
            }

            SharedInventorySync[] warehouses = FindObjectsOfType<SharedInventorySync>();
            foreach (var wh in warehouses)
            {
                registeredWarehouses[wh.cityID] = wh;
            }

            Debug.Log($"[GameManager SERVER] Multi-city registry rebuilt: {registeredReactors.Count} Reactors, {registeredWalls.Count} Walls, {registeredWarehouses.Count} Warehouses.");
        }

        [Server]
        private void OnServerCityReactorFrozen(int cityID)
        {
            if (defeatedCityIDs.Contains(cityID)) return;

            defeatedCityIDs.Add(cityID);
            string reason = $"EL REACTOR DE LA CIUDAD {cityID} SE CONGELÓ COMPLETAMENTE (-50°C). LA CALDERA {cityID} HA CAÍDO.";
            Debug.LogError($"[GameManager SERVER] CITY {cityID} DEFEATED: {reason}");

            RpcNotifyCityDefeated(cityID, reason);

            // If ALL registered cities are defeated, trigger global game over
            if (defeatedCityIDs.Count >= registeredReactors.Count && registeredReactors.Count > 0)
            {
                TriggerTeamDefeat("TODAS LAS CALDERAS HAN SIDO CONGELADAS. LA HUMANIDAD HA CAÍDO.");
            }
        }

        /// <summary>
        /// Returns whether the specified cityID has suffered a total freeze defeat.
        /// </summary>
        public bool IsCityDefeated(int cityID)
        {
            return defeatedCityIDs.Contains(cityID);
        }

        public ReactorManager GetReactorForCity(int cityID)
        {
            if (registeredReactors.TryGetValue(cityID, out ReactorManager r) && r != null) return r;
            RebuildMultiCityRegistries();
            registeredReactors.TryGetValue(cityID, out r);
            return r;
        }

        public CityWallHealthSync GetWallForCity(int cityID)
        {
            if (registeredWalls.TryGetValue(cityID, out CityWallHealthSync w) && w != null) return w;
            RebuildMultiCityRegistries();
            registeredWalls.TryGetValue(cityID, out w);
            return w;
        }

        public SharedInventorySync GetWarehouseForCity(int cityID)
        {
            if (registeredWarehouses.TryGetValue(cityID, out SharedInventorySync wh) && wh != null) return wh;
            RebuildMultiCityRegistries();
            registeredWarehouses.TryGetValue(cityID, out wh);
            return wh;
        }

        private void Update()
        {
            if (!isServer || currentPhase == GamePhase.GameOver) return;

            phaseTimer -= Time.deltaTime;

            if (phaseTimer <= 0f)
            {
                AdvanceGameCycle();
            }
        }

        [Server]
        private void AdvanceGameCycle()
        {
            switch (currentPhase)
            {
                case GamePhase.ExpeditionPhase:
                    Debug.Log("[GameManager SERVER] EXPEDITION PHASE ENDED. NIEBLA HELADA DENSIFYING -> ENTERING SIEGE PHASE!");
                    TransitionToPhase(GamePhase.SiegePhase);
                    break;

                case GamePhase.SiegePhase:
                    Debug.Log("[GameManager SERVER] SIEGE PHASE SURVIVED! OPENING COUNCIL VOTING SESSION...");
                    TransitionToPhase(GamePhase.CouncilPhase);
                    if (CouncilVotingManager.Instance != null)
                    {
                        CouncilVotingManager.Instance.StartVotingSession("Distribución de Energía Térmica", "Asignar la potencia restante entre Invernaderos, Escudos de Muralla o Reactor.");
                    }
                    break;

                case GamePhase.CouncilPhase:
                    Debug.Log("[GameManager SERVER] COUNCIL VOTING CONCLUDED -> STARTING NEW EXPEDITION CYCLE.");
                    TransitionToPhase(GamePhase.ExpeditionPhase);
                    break;
            }
        }

        [Server]
        public void TransitionToPhase(GamePhase newPhase)
        {
            currentPhase = newPhase;

            switch (newPhase)
            {
                case GamePhase.ExpeditionPhase:
                    phaseTimer = expeditionPhaseDuration;
                    break;
                case GamePhase.SiegePhase:
                    phaseTimer = siegePhaseDuration;
                    break;
                case GamePhase.CouncilPhase:
                    phaseTimer = 30f;
                    break;
            }
        }

        private void OnPhaseSyncHook(GamePhase oldVal, GamePhase newVal)
        {
            OnPhaseChanged?.Invoke(newVal);
        }

        [Server]
        public void TriggerTeamDefeat(string reason)
        {
            if (currentPhase == GamePhase.GameOver) return;

            currentPhase = GamePhase.GameOver;
            Debug.LogError($"[GameManager SERVER] ALL-CITIES GAME OVER TRIGGERED: {reason}");
            RpcNotifyTeamDefeat(reason);
        }

        [Server]
        public void TriggerTeamVictory()
        {
            if (currentPhase == GamePhase.GameOver) return;

            currentPhase = GamePhase.GameOver;
            Debug.Log("[GameManager SERVER] TEAM VICTORY! LA CALDERA SURVIVED THE WINTER STORM!");
            RpcNotifyTeamVictory();
        }

        [ClientRpc]
        private void RpcNotifyCityDefeated(int cityID, string reason)
        {
            Debug.LogError($"[GameManager CLIENT] CITY {cityID} DEFEATED: {reason}");
            OnCityDefeated?.Invoke(cityID, reason);
        }

        [ClientRpc]
        private void RpcNotifyTeamDefeat(string reason)
        {
            OnTeamDefeat?.Invoke(reason);
        }

        [ClientRpc]
        private void RpcNotifyTeamVictory()
        {
            OnTeamVictory?.Invoke();
        }
    }
}
