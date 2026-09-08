using UnityEngine;
using UnityEngine.AI;
using EcoDeLasCenizas.Networking;
using EcoDeLasCenizas.Gameplay;

namespace EcoDeLasCenizas.AI
{
    /// <summary>
    /// AI controller for 'Sombras Heladas' (Frozen Shadow creatures).
    /// Uses NavMeshAgent to dynamically locate and advance toward the City Wall section
    /// with the lowest remaining HP for its target cityID during siege phases.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Target City Faction")]
        [SerializeField] private int targetCityID = 1;

        [Header("Combat & Attack Parameters")]
        [SerializeField] private float attackDamage = 25.0f;
        [SerializeField] private float attackInterval = 2.0f;
        [SerializeField] private float attackDistance = 3.5f;

        [Header("Targeting Settings")]
        [SerializeField] private WallSection currentTargetWallSection = WallSection.North;
        [SerializeField] private Transform[] wallSectionWaypoints; // Assign N, S, E, W waypoints in Inspector

        private NavMeshAgent navAgent;
        private float nextAttackTimer = 0f;
        private bool isAttacking = false;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            RecalculateTargetWallSection();
        }

        private void Update()
        {
            if (navAgent == null || !navAgent.enabled) return;

            if (Time.frameCount % 60 == 0)
            {
                RecalculateTargetWallSection();
            }

            if (navAgent.remainingDistance <= attackDistance && !navAgent.pathPending)
            {
                isAttacking = true;
                ExecuteWallAttack();
            }
            else
            {
                isAttacking = false;
            }
        }

        /// <summary>
        /// Finds the city wall section with the lowest HP for targetCityID and sets it as destination.
        /// </summary>
        public void RecalculateTargetWallSection()
        {
            CityWallHealthSync wallSync = GameManager.Instance != null ? GameManager.Instance.GetWallForCity(targetCityID) : FindObjectOfType<CityWallHealthSync>();
            if (wallSync == null) return;

            WallSection lowestSection = WallSection.North;
            float lowestHP = float.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                WallSection section = (WallSection)i;
                WallStatus status = wallSync.GetWallStatus(section);

                if (status.currentHP < lowestHP && status.currentHP > 0f)
                {
                    lowestHP = status.currentHP;
                    lowestSection = section;
                }
            }

            currentTargetWallSection = lowestSection;

            int waypointIndex = (int)currentTargetWallSection;
            if (wallSectionWaypoints != null && waypointIndex < wallSectionWaypoints.Length && wallSectionWaypoints[waypointIndex] != null)
            {
                navAgent.SetDestination(wallSectionWaypoints[waypointIndex].position);
            }
            else
            {
                navAgent.SetDestination(Vector3.zero);
            }
        }

        private void ExecuteWallAttack()
        {
            if (Time.time >= nextAttackTimer)
            {
                nextAttackTimer = Time.time + attackInterval;

                Debug.Log($"[EnemyAI] Sombra Helada ATTACKING {currentTargetWallSection} Wall on City {targetCityID} for {attackDamage} DMG!");

                CityWallHealthSync wallSync = GameManager.Instance != null ? GameManager.Instance.GetWallForCity(targetCityID) : FindObjectOfType<CityWallHealthSync>();
                if (wallSync != null)
                {
                    wallSync.DamageWall(currentTargetWallSection, attackDamage, -1);
                }
            }
        }

        public WallSection CurrentTarget => currentTargetWallSection;
        public bool IsAttacking => isAttacking;
    }
}
