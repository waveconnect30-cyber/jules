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
    /// Core Game Manager orchestrating phase cycles (Expedition vs. Siege vs. Council),
    /// time management, and global team win/loss conditions.
    /// Phase transitions execute strictly on [Server] and sync to clients via [SyncVar].
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Phase Management")]
        [SyncVar(hook = nameof(OnPhaseSyncHook))] [SerializeField] private GamePhase currentPhase = GamePhase.ExpeditionPhase;
        [SerializeField] private float expeditionPhaseDuration = 180f; // 3 minutes
        [SerializeField] private float siegePhaseDuration = 120f;      // 2 minutes
        [SyncVar] private float phaseTimer = 0f;

        [Header("Events")]
        public UnityEvent<GamePhase> OnPhaseChanged;
        public UnityEvent OnTeamVictory;
        public UnityEvent<string> OnTeamDefeat;

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

            // Subscribe to defeat triggers on server
            if (ReactorManager.Instance != null)
            {
                ReactorManager.Instance.OnCityFrozenSolid.AddListener(() => TriggerTeamDefeat("EL REACTOR SE CONGELÓ COMPLETAMENTE (-50°C). LA CALDERA HA CAÍDO."));
            }

            if (CityWallHealthSync.Instance != null)
            {
                CityWallHealthSync.Instance.OnWallBreached += (section) =>
                {
                    Debug.LogWarning($"[GameManager SERVER] {section} Wall breached! Entering emergency Siege phase.");
                    if (currentPhase == GamePhase.ExpeditionPhase)
                    {
                        TransitionToPhase(GamePhase.SiegePhase);
                    }
                };
            }
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
            Debug.LogError($"[GameManager SERVER] TEAM DEFEAT TRIGGERED: {reason}");
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
