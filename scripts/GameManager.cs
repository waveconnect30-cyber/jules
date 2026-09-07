using UnityEngine;
using UnityEngine.Events;
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
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Phase Management")]
        [SerializeField] private GamePhase currentPhase = GamePhase.ExpeditionPhase;
        [SerializeField] private float expeditionPhaseDuration = 180f; // 3 minutes
        [SerializeField] private float siegePhaseDuration = 120f;      // 2 minutes
        private float phaseTimer = 0f;

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

        private void Start()
        {
            TransitionToPhase(GamePhase.ExpeditionPhase);

            // Subscribe to defeat triggers
            if (ReactorManager.Instance != null)
            {
                ReactorManager.Instance.OnCityFrozenSolid.AddListener(() => TriggerTeamDefeat("EL REACTOR SE CONGELÓ COMPLETAMENTE (-50°C). LA CALDERA HA CAÍDO."));
            }

            if (CityWallHealthSync.Instance != null)
            {
                CityWallHealthSync.Instance.OnWallBreached += (section) =>
                {
                    Debug.LogWarning($"[GameManager] {section} Wall breached! Entering emergency Siege phase.");
                    if (currentPhase == GamePhase.ExpeditionPhase)
                    {
                        TransitionToPhase(GamePhase.SiegePhase);
                    }
                };
            }
        }

        private void Update()
        {
            if (currentPhase == GamePhase.GameOver) return;

            phaseTimer -= Time.deltaTime;

            if (phaseTimer <= 0f)
            {
                AdvanceGameCycle();
            }
        }

        private void AdvanceGameCycle()
        {
            switch (currentPhase)
            {
                case GamePhase.ExpeditionPhase:
                    Debug.Log("[GameManager] EXPEDITION PHASE ENDED. NIEBLA HELADA DENSIFYING -> ENTERING SIEGE PHASE!");
                    TransitionToPhase(GamePhase.SiegePhase);
                    break;

                case GamePhase.SiegePhase:
                    Debug.Log("[GameManager] SIEGE PHASE SURVIVED! OPENING COUNCIL VOTING SESSION...");
                    TransitionToPhase(GamePhase.CouncilPhase);
                    if (CouncilVotingManager.Instance != null)
                    {
                        CouncilVotingManager.Instance.StartVotingSession("Distribución de Energía Térmica", "Asignar la potencia restante entre Invernaderos, Escudos de Muralla o Reactor.");
                    }
                    break;

                case GamePhase.CouncilPhase:
                    Debug.Log("[GameManager] COUNCIL VOTING CONCLUDED -> STARTING NEW EXPEDITION CYCLE.");
                    TransitionToPhase(GamePhase.ExpeditionPhase);
                    break;
            }
        }

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

            OnPhaseChanged?.Invoke(currentPhase);
        }

        public void TriggerTeamDefeat(string reason)
        {
            if (currentPhase == GamePhase.GameOver) return;

            currentPhase = GamePhase.GameOver;
            Debug.LogError($"[GameManager] TEAM DEFEAT TRIGGERED: {reason}");
            OnTeamDefeat?.Invoke(reason);
        }

        public void TriggerTeamVictory()
        {
            if (currentPhase == GamePhase.GameOver) return;

            currentPhase = GamePhase.GameOver;
            Debug.Log("[GameManager] TEAM VICTORY! LA CALDERA SURVIVED THE WINTER STORM!");
            OnTeamVictory?.Invoke();
        }
    }
}
