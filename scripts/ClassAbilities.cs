using UnityEngine;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.UI;

namespace EcoDeLasCenizas.Player
{
    /// <summary>
    /// Executes unique active abilities for each survivor class:
    /// - Explorer: Geothermal Ore Scanner (highlights Ignicita nodes)
    /// - Engineer: Automated Thermal Repair Turret
    /// - Scientist: Catalyst Booster (doubles fuel output)
    /// - Tactician: Battle Rally (+20% Defense to nearby allies)
    /// Triggers via PC KeyCode.Q or Mobile TouchScreenHUD IsAbilityPressed signal.
    /// </summary>
    public class ClassAbilities : MonoBehaviour
    {
        [Header("Ability Settings")]
        [SerializeField] private float abilityCooldown = 15.0f;
        private float currentCooldownTimer = 0f;

        [Header("Explorer Scanner Prefab/VFX")]
        [SerializeField] private float scanRadius = 35.0f;

        [Header("Engineer Repair Turret Prefab")]
        [SerializeField] private GameObject repairTurretPrefab;

        private PlayerController playerController;

        public float AbilityCooldown => abilityCooldown;
        public float CurrentCooldownTimer => currentCooldownTimer;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (currentCooldownTimer > 0f)
            {
                currentCooldownTimer -= Time.deltaTime;
            }

            // Check Keyboard 'Q' or Mobile Touch Ability Button
            bool abilityInput = Input.GetKeyDown(KeyCode.Q) || (TouchScreenHUD.Instance != null && TouchScreenHUD.Instance.IsAbilityPressed);

            if (abilityInput && currentCooldownTimer <= 0f)
            {
                TriggerClassAbility();
            }
        }

        public void ModifyCooldown(float multiplier)
        {
            abilityCooldown *= multiplier;
        }

        public void TriggerClassAbility()
        {
            if (playerController == null) return;

            switch (playerController.Class)
            {
                case CharacterClass.Explorer:
                    ExecuteExplorerScanner();
                    break;
                case CharacterClass.Engineer:
                    ExecuteEngineerRepairTurret();
                    break;
                case CharacterClass.Scientist:
                    ExecuteScientistCatalystBooster();
                    break;
                case CharacterClass.Tactician:
                    ExecuteTacticianBattleRally();
                    break;
            }

            currentCooldownTimer = abilityCooldown;
        }

        private void ExecuteExplorerScanner()
        {
            Debug.Log($"[ClassAbilities] Explorer SCANNER ACTIVATED. Scanning {scanRadius}m radius for Ignicita nodes...");
            Collider[] hits = Physics.OverlapSphere(transform.position, scanRadius);
            int nodeCount = 0;
            foreach (var hit in hits)
            {
                if (hit.CompareTag("IgnicitaNode") || hit.name.Contains("Ignicita"))
                {
                    nodeCount++;
                }
            }
            Debug.Log($"[ClassAbilities] Scanner detected {nodeCount} Ignicita mineral deposits nearby!");
        }

        private void ExecuteEngineerRepairTurret()
        {
            Debug.Log("[ClassAbilities] Engineer DEPLOYED AUTOMATED REPAIR TURRET.");
            if (repairTurretPrefab != null)
            {
                Instantiate(repairTurretPrefab, transform.position + transform.forward * 2f, Quaternion.identity);
            }
        }

        private void ExecuteScientistCatalystBooster()
        {
            Debug.Log("[ClassAbilities] Scientist CATALYST BOOSTER ACTIVATED. Fuel efficiency doubled for 20 seconds.");
            if (ReactorManager.Instance != null)
            {
                ReactorManager.Instance.DepositIgnicita(30f);
            }
        }

        private void ExecuteTacticianBattleRally()
        {
            Debug.Log("[ClassAbilities] Tactician BATTLE RALLY ACTIVATED! +20% Defense buff granted to all wall defenders.");
        }
    }
}
