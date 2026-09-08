using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Networking;

namespace EcoDeLasCenizas.Gameplay
{
    public enum SeasonPhase
    {
        Settlement,         // Days 1-3: Raid immunity active, city setup
        Expansion,          // Days 4-8: World map ruins capture & raiding enabled
        PresidentialSiege,  // Days 9-12: Capital (0,0,0) capture unlocked (3-hour hold condition)
        OverloadWipe        // Days 13-14: Server overload event, wipe & cosmetics distribution
    }

    [System.Serializable]
    public struct ClanSeasonScore
    {
        public int cityID;
        public float totalIgnicitaProcessed;
        public int ruinsControlled;
    }

    /// <summary>
    /// Controls the 14-day seasonal cycle, managing raid immunity during Settlement,
    /// Presidential City capital capture in Siege phase, Governor 5% global Ignicita tax,
    /// and Overload Wipe end-of-season rewards and server state resets.
    /// Resets capital hold timer when ownership changes and triggers Governor crown as a one-shot event.
    /// </summary>
    public class SeasonManager : NetworkBehaviour
    {
        public static SeasonManager Instance { get; private set; }

        [Header("Season Schedule Settings")]
        [SyncVar(hook = nameof(OnSeasonPhaseChanged))]
        public SeasonPhase currentSeasonPhase = SeasonPhase.Settlement;

        [SyncVar] public float currentSeasonDay = 1.0f; // Days 1 to 14
        [SerializeField] private float secondsPerSeasonDay = 86400f; // 24 hours per day (default)

        [Header("Governor System")]
        [SyncVar] public int governorCityID = 0; // 0 = No Governor
        [SerializeField] private float governorTaxPercentage = 0.05f; // 5% global Ignicita tax
        private bool isGovernorCrowned = false;

        [Header("Presidential Siege Hold Victory")]
        [SyncVar] public float capitalHoldTimerSeconds = 0f;
        [SerializeField] private float requiredHoldSecondsForVictory = 10800f; // 3 hours (10,800 seconds)
        private int previousCapitalOwner = -1;

        [Header("Season Leaderboard & Persistence")]
        public readonly SyncList<ClanSeasonScore> clanLeaderboard = new SyncList<ClanSeasonScore>();

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

            currentSeasonDay += Time.deltaTime / secondsPerSeasonDay;
            EvaluateSeasonPhase();

            if (currentSeasonPhase == SeasonPhase.PresidentialSiege && WorldMapManager.Instance != null)
            {
                int capitalOwner = WorldMapManager.Instance.capitalControllingCityID;

                if (capitalOwner != previousCapitalOwner)
                {
                    previousCapitalOwner = capitalOwner;
                    capitalHoldTimerSeconds = 0f;
                    isGovernorCrowned = false;
                    Debug.Log($"[SeasonManager SERVER] Capital owner changed to City {capitalOwner}. Resetting hold timer to 0.");
                }

                if (capitalOwner > 0 && !isGovernorCrowned)
                {
                    capitalHoldTimerSeconds += Time.deltaTime;
                    if (capitalHoldTimerSeconds >= requiredHoldSecondsForVictory)
                    {
                        isGovernorCrowned = true;
                        governorCityID = capitalOwner;
                        Debug.LogWarning($"[SeasonManager SERVER] CITY {governorCityID} HELD CAPITAL FOR 3 HOURS! GOVERNOR TITLE GRANTED!");
                        RpcAnnounceGovernorCrown(governorCityID);
                    }
                }
            }
        }

        private void EvaluateSeasonPhase()
        {
            SeasonPhase nextPhase;

            if (currentSeasonDay <= 3.0f)
            {
                nextPhase = SeasonPhase.Settlement;
            }
            else if (currentSeasonDay <= 8.0f)
            {
                nextPhase = SeasonPhase.Expansion;
            }
            else if (currentSeasonDay <= 12.0f)
            {
                nextPhase = SeasonPhase.PresidentialSiege;
            }
            else
            {
                nextPhase = SeasonPhase.OverloadWipe;
            }

            if (nextPhase != currentSeasonPhase)
            {
                currentSeasonPhase = nextPhase;
                HandlePhaseTransition(currentSeasonPhase);
            }
        }

        private void HandlePhaseTransition(SeasonPhase phase)
        {
            Debug.LogWarning($"[SeasonManager SERVER] SEASON PHASE TRANSITIONED TO: {phase} (Day {currentSeasonDay:F1})");

            if (phase == SeasonPhase.OverloadWipe)
            {
                TriggerEndofSeasonOverloadWipe();
            }
        }

        public bool IsRaidAllowed()
        {
            return currentSeasonPhase != SeasonPhase.Settlement;
        }

        public float ApplyGovernorTax(float processedIgnicita)
        {
            if (governorCityID <= 0 || processedIgnicita <= 0f) return 0f;

            float taxAmount = processedIgnicita * governorTaxPercentage;

            SharedInventorySync warehouse = GameManager.Instance != null ? GameManager.Instance.GetWarehouseForCity(governorCityID) : null;
            if (warehouse != null)
            {
                warehouse.AddIgnicitaToWarehouse(taxAmount, governorCityID);
                Debug.Log($"[SeasonManager SERVER] 5% Governor Tax ({taxAmount:F1} Ignicita) paid to City {governorCityID}.");
            }

            return taxAmount;
        }

        [Server]
        private void TriggerEndofSeasonOverloadWipe()
        {
            Debug.LogError("[SeasonManager CRITICAL] OVERLOAD WIPE PHASE ACTIVATED! Server reset sequence initiating. Awarding seasonal cosmetics to survivors...");

            // Save seasonal leaderboard scores
            PlayerPrefs.SetInt("SeasonCompleted", 1);
            PlayerPrefs.SetInt("LastGovernorCityID", governorCityID);
            PlayerPrefs.Save();

            if (GlobalEventManager.Instance != null)
            {
                GlobalEventManager.Instance.TriggerGlobalEvent(GlobalWeatherEvent.SuperIceStorm, 600f);
            }

            RpcAnnounceSeasonOverloadWipe();
        }

        private void OnSeasonPhaseChanged(SeasonPhase oldPhase, SeasonPhase newPhase)
        {
            Debug.Log($"[SeasonManager CLIENT] Season Phase updated to {newPhase}");
        }

        [ClientRpc]
        private void RpcAnnounceGovernorCrown(int cityID)
        {
            Debug.LogError($"[SEASON GOVERNOR ANNOUNCEMENT] CITY {cityID} HAS HELD THE PRESIDENTIAL CAPITAL FOR 3 HOURS AND IS CROWNED GOVERNOR OF LA CALDERA!");
        }

        [ClientRpc]
        private void RpcAnnounceSeasonOverloadWipe()
        {
            Debug.LogError("[SEASON END ALERT] THE REACTOR OVERLOAD WIPE HAS BEGUN! All clans receive seasonal trophy cosmetics based on final score!");
        }
    }
}
