using UnityEngine;

namespace EcoDeLasCenizas.Core
{
    /// <summary>
    /// Interface for interactive objects in the world (Reactor container, Wall repair stations, Resource crates).
    /// </summary>
    public interface IInteractable
    {
        string GetInteractionPrompt();
        void Interact(PlayerController player);
    }
}

namespace EcoDeLasCenizas.Player
{
    using EcoDeLasCenizas.Gameplay;
    using EcoDeLasCenizas.Core;

    /// <summary>
    /// Comprehensive 3D Player Controller handling third-person locomotion, jumping,
    /// raycast interaction with IInteractable objects, and dynamic freezing speed penalties (-20%).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Class & Identity")]
        [SerializeField] private CharacterClass characterClass = CharacterClass.Explorer;

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
        [SerializeField] private float carriedIgnicitaAmount = 25.0f;

        // Internal State
        private CharacterController characterController;
        private Vector3 velocity;
        private bool isGrounded;
        private float movementSpeedMultiplier = 1.0f;

        // Public Properties
        public CharacterClass Class => characterClass;
        public float CarriedIgnicita => carriedIgnicitaAmount;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            ApplyClassStats();

            if (ReactorManager.Instance != null)
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
            HandleGroundCheck();
            HandleLocomotion();
            HandleJump();
            HandleInteraction();
            ApplyGravity();
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
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
            }
        }

        private void HandleInteraction()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Ray ray = cameraTransform
                    ? new Ray(cameraTransform.position, cameraTransform.forward)
                    : new Ray(transform.position, transform.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayerMask))
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        Debug.Log($"[PlayerController] Interacting with {hit.collider.gameObject.name}");
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
            Debug.Log($"[PlayerController] Movement speed multiplier set to {movementSpeedMultiplier * 100}% due to freezing alert ({isFreezingActive}).");
        }
    }
}
