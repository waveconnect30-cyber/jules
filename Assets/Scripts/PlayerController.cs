using UnityEngine;
using Mirror;
using EcoDeLasCenizas.UI;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Core;

namespace EcoDeLasCenizas.Player
{
    /// <summary>
    /// Cross-platform 3D Player Controller with Mirror network authority protection, SyncVars,
    /// server-authoritative death/respawn, and adaptive controls (PC & Android).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Faction & City Ownership")]
        [Tooltip("ID of the city/clan this survivor belongs to in PvPvE mode.")]
        [SyncVar] public int cityID = 1;

        [Header("Player Health & Health State")]
        [SyncVar(hook = nameof(OnCurrentHPSyncHook))] [SerializeField] private float currentHP = 100.0f;
        [SyncVar] [SerializeField] private float maxHP = 100.0f;
        [SyncVar] private bool isDead = false;

        [Header("Class & Identity")]
        [SyncVar] [SerializeField] private CharacterClass characterClass = CharacterClass.Explorer;

        [Header("Movement Configuration")]
        [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.5f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -19.62f;

        [Header("Interaction System")]
        [SerializeField] private float interactionDistance = 3.0f;
        [SerializeField] private LayerMask interactableLayerMask;
        [SerializeField] private Transform cameraTransform;

        [Header("Inventory Carrying")]
        [SyncVar] [SerializeField] private float carriedIgnicitaAmount = 25.0f;

        // Internal State
        private CharacterController characterController;
        private Vector3 velocity;
        private bool isGrounded;
        private float movementSpeedMultiplier = 1.0f;

        // Public Properties
        public CharacterClass Class => characterClass;
        public float CarriedIgnicita => carriedIgnicitaAmount;
        public float CurrentHP => currentHP;
        public float MaxHP => maxHP;
        public bool IsDead => isDead;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            ApplyClassStats();

            if (ReactorManager.Instance != null && ReactorManager.Instance.CityID == cityID)
            {
                ReactorManager.Instance.OnPlayerMovementPenaltyChanged.AddListener(OnFreezingPenaltyUpdated);
                OnFreezingPenaltyUpdated(ReactorManager.Instance.IsMovementSlowed);
            }
        }

        private void ApplyClassStats()
        {
            switch (characterClass)
            {
                case CharacterClass.Explorer:
                    walkSpeed = 6.0f;
                    sprintSpeed = 9.5f;
                    break;
                case CharacterClass.Engineer:
                    walkSpeed = 4.5f;
                    sprintSpeed = 7.0f;
                    break;
                case CharacterClass.Scientist:
                    walkSpeed = 4.8f;
                    sprintSpeed = 7.5f;
                    break;
                case CharacterClass.Tactician:
                    walkSpeed = 5.0f;
                    sprintSpeed = 8.0f;
                    break;
            }
        }

        private void Update()
        {
            if (isDead) return;

            // NETWORK AUTHORITY CHECK: Only local controlling player processes input and locomotion
            if (isServer || isLocalPlayer)
            {
                HandleGroundCheck();

                if (isLocalPlayer)
                {
                    HandleLocomotion();
                    HandleJump();
                    HandleInteraction();
                }

                ApplyGravity();
            }
        }

        private void HandleGroundCheck()
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2.0f;
            }
        }

        private void HandleLocomotion()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (TouchScreenHUD.Instance != null && TouchScreenHUD.Instance.JoystickInput != Vector2.zero)
            {
                horizontal = TouchScreenHUD.Instance.JoystickInput.x;
                vertical = TouchScreenHUD.Instance.JoystickInput.y;
            }

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            if (direction.magnitude >= 0.1f)
            {
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + (cameraTransform ? cameraTransform.eulerAngles.y : 0f);
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                bool isSprinting = Input.GetKey(KeyCode.LeftShift);
                float speed = (isSprinting ? sprintSpeed : walkSpeed) * movementSpeedMultiplier;

                characterController.Move(moveDir.normalized * speed * Time.deltaTime);
            }
        }

        private void HandleJump()
        {
            bool jumpInput = Input.GetButtonDown("Jump") || (TouchScreenHUD.Instance != null && TouchScreenHUD.Instance.IsJumpPressed);

            if (jumpInput && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
            }
        }

        private void HandleInteraction()
        {
            bool interactInput = Input.GetKeyDown(KeyCode.F) || (TouchScreenHUD.Instance != null && TouchScreenHUD.Instance.IsInteractPressed);

            if (interactInput)
            {
                Ray ray = cameraTransform
                    ? new Ray(cameraTransform.position, cameraTransform.forward)
                    : new Ray(transform.position, transform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayerMask))
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        Debug.Log($"[PlayerController City:{cityID}] Interacting with {hit.collider.gameObject.name}");
                        interactable.Interact(this);
                    }
                }
            }
        }

        private void ApplyGravity()
        {
            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }

        [Server]
        public void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHP = Mathf.Max(0f, currentHP - damage);
            Debug.LogWarning($"[PlayerController SERVER] {name} took {damage} HP damage. Remaining: {currentHP}/{maxHP}");

            if (currentHP <= 0f)
            {
                DieAndRespawn();
            }
        }

        [Server]
        private void DieAndRespawn()
        {
            isDead = true;
            Debug.LogError($"[PlayerController SERVER] PLAYER DIED: {name} (CityID:{cityID}). Initiating respawn timer...");
            RpcOnPlayerDeath();

            Invoke(nameof(RespawnAtCitySpawnPoint), 5.0f);
        }

        [Server]
        private void RespawnAtCitySpawnPoint()
        {
            currentHP = maxHP;
            isDead = false;

            // Teleport to spawn point (or origin)
            transform.position = new Vector3(cityID * 10f, 0f, 0f);
            Debug.Log($"[PlayerController SERVER] PLAYER RESPAWNED: {name} (CityID:{cityID}) at {transform.position}");
            RpcOnPlayerRespawn(transform.position);
        }

        [ClientRpc]
        private void RpcOnPlayerDeath()
        {
            if (characterController != null) characterController.enabled = false;
            Debug.LogWarning($"[PlayerController CLIENT] Local Player Dead. Control disabled.");
        }

        [ClientRpc]
        private void RpcOnPlayerRespawn(Vector3 spawnPos)
        {
            transform.position = spawnPos;
            if (characterController != null) characterController.enabled = true;
            Debug.Log($"[PlayerController CLIENT] Local Player Respawned. Control re-enabled.");
        }

        private void OnCurrentHPSyncHook(float oldVal, float newVal)
        {
            Debug.Log($"[PlayerController SyncVar] HP synced: {newVal}/{maxHP}");
        }

        public void ConsumeCarriedIgnicita(float amount)
        {
            carriedIgnicitaAmount = Mathf.Max(0f, carriedIgnicitaAmount - amount);
        }

        public void AddCarriedIgnicita(float amount)
        {
            carriedIgnicitaAmount += amount;
        }

        public void OnFreezingPenaltyUpdated(bool isFreezingActive)
        {
            movementSpeedMultiplier = isFreezingActive ? 0.80f : 1.0f;
            Debug.Log($"[PlayerController City:{cityID}] Movement speed multiplier set to {movementSpeedMultiplier * 100}% due to freezing alert ({isFreezingActive}).");
        }
    }
}
