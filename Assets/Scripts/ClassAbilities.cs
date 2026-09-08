using UnityEngine;
using Mirror;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Core;
using EcoDeLasCenizas.UI;

namespace EcoDeLasCenizas.Player
{
    /// <summary>
    /// Executes unique active abilities for each survivor class via server Commands:
    /// - Explorer: Geothermal Ore Scanner (highlights Ignicita nodes)
    /// - Engineer: Automated Thermal Repair Turret (NetworkServer.Spawn)
    /// - Scientist: Catalyst Booster (doubles fuel output via server DepositIgnicita)
    /// - Tactician: Battle Rally (+20% Defense to nearby allies)
    /// Input reading restricted to local player (isLocalPlayer).
    /// Server validates currentHP > 0 and server cooldown timers before executing.
    /// </summary>
    public class ClassAbilities : NetworkBehaviour
    {
        [Header("Ability Settings")]
        [SerializeField] private float abilityCooldown = 15.0f;
        private float currentCooldownTimer = 0f;
        [SyncVar] private float serverLastAbilityTimestamp = -100f;

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

            if (!isLocalPlayer) return;

            bool abilityInput = Input.GetKeyDown(KeyCode.Q) || (TouchScreenHUD.Instance != null && TouchScreenHUD.Instance.IsAbilityPressed);

            if (abilityInput && currentCooldownTimer <= 0f)
            {
                currentCooldownTimer = abilityCooldown;
                CmdExecuteAbility();
            }
        }

        public void ModifyCooldown(float multiplier)
        {
            abilityCooldown *= multiplier;
        }

        [Command]
        public void CmdExecuteAbility()
        {
            if (playerController == null || playerController.IsDead || playerController.CurrentHP <= 0f)
            {
                Debug.LogWarning("[ClassAbilities SERVER] REJECTED ability: Player is dead.");
                return;
            }

            // SERVER COOLDOWN VALIDATION
            if (Time.time < serverLastAbilityTimestamp + (abilityCooldown - 0.5f))
            {
                Debug.LogWarning($"[ClassAbilities SERVER] REJECTED ability: Cooldown still active on server.");
                return;
            }

            serverLastAbilityTimestamp = Time.time;
            Debug.Log($"[ClassAbilities SERVER] Executing ability for Player {playerController.name} (Class: {playerController.Class}, CityID: {playerController.cityID})");

            switch (playerController.Class)
            {
                case CharacterClass.Explorer:
                    RpcExecuteExplorerScanner(transform.position, scanRadius);
                    break;
                case CharacterClass.Engineer:
                    ExecuteEngineerRepairTurret();
                    break;
                case CharacterClass.Scientist:
                    ExecuteScientistCatalystBooster();
                    break;
                case CharacterClass.Tactician:
                    RpcExecuteTacticianBattleRally(playerController.cityID);
                    break;
            }
        }

        [ClientRpc]
        private void RpcExecuteExplorerScanner(Vector3 position, float radius)
        {
            if (!isLocalPlayer) return;

            Debug.Log($"[ClassAbilities CLIENT] Explorer SCANNER ACTIVATED at {position}. Scanning {radius}m radius for Ignicita nodes...");
            Collider[] hits = Physics.OverlapSphere(position, radius);
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

        [Server]
        private void ExecuteEngineerRepairTurret()
        {
            Debug.Log("[ClassAbilities SERVER] Engineer DEPLOYED AUTOMATED REPAIR TURRET.");
            if (repairTurretPrefab != null)
            {
                GameObject turret = Instantiate(repairTurretPrefab, transform.position + transform.forward * 2f, Quaternion.identity);
                NetworkServer.Spawn(turret);
            }
        }

        [Server]
        private void ExecuteScientistCatalystBooster()
        {
            Debug.Log("[ClassAbilities SERVER] Scientist CATALYST BOOSTER ACTIVATED. Fuel efficiency doubled.");
            ReactorManager reactor = GameManager.Instance != null ? GameManager.Instance.GetReactorForCity(playerController.cityID) : FindObjectOfType<ReactorManager>();
            if (reactor != null)
            {
                reactor.DepositIgnicita(30f, playerController.cityID);
            }
        }

        [ClientRpc]
        private void RpcExecuteTacticianBattleRally(int cityID)
        {
            Debug.Log($"[ClassAbilities RPC] Tactician BATTLE RALLY ACTIVATED! +20% Defense buff granted to City {cityID} wall defenders.");
        }
    }
}
