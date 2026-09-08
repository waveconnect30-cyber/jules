using UnityEngine;
using UnityEngine.AI;
using EcoDeLasCenizas.Networking;

namespace EcoDeLasCenizas.AI
{
    /// <summary>
    /// AI controller for 'Sombras Heladas' (Frozen Shadow creatures).
    /// Uses NavMeshAgent to dynamically locate and advance toward the City Wall section
    /// with the lowest remaining HP during siege phases. Attacks walls upon reaching melee distance.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
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

            // Periodically check if another wall section has lower HP
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
        /// Finds the city wall section with the lowest HP and sets it as the NavMesh destination.
        /// </summary>
        public void RecalculateTargetWallSection()
        {
            if (CityWallHealthSync.Instance == null) return;

            WallSection lowestSection = WallSection.North;
            float lowestHP = float.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                WallSection section = (WallSection)i;
                WallStatus status = CityWallHealthSync.Instance.GetWallStatus(section);

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
                // Fallback movement toward origin/wall direction if waypoints not assigned
                navAgent.SetDestination(Vector3.zero);
            }
        }

        private void ExecuteWallAttack()
        {
            if (Time.time >= nextAttackTimer)
            {
                nextAttackTimer = Time.time + attackInterval;

                Debug.Log($"[EnemyAI] Sombra Helada ATTACKING {currentTargetWallSection} Wall for {attackDamage} DMG!");

                if (CityWallHealthSync.Instance != null)
                {
                    CityWallHealthSync.Instance.DamageWall(currentTargetWallSection, attackDamage);
                }
            }
        }

        public WallSection CurrentTarget => currentTargetWallSection;
        public bool IsAttacking => isAttacking;
    }
}
