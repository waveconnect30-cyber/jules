using UnityEngine;
using EcoDeLasCenizas.Gameplay;
using EcoDeLasCenizas.Core;

namespace EcoDeLasCenizas.Player
{
    /// <summary>
    /// 3D Character Controller for survival movement, class-based mobility (Explorer grappling hook),
    /// sprinting, jumping, and dynamic speed penalties during city freezing (-10°C).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacterController : MonoBehaviour
    {
        [Header("Faction Configuration")]
        [SerializeField] private int cityID = 1;

        [Header("Class Configuration")]
        [SerializeField] private CharacterClass selectedClass = CharacterClass.Explorer;

        [Header("Base Movement Settings")]
        [SerializeField] private float walkSpeed = 5.0f;
        [SerializeField] private float sprintSpeed = 8.0f;
        [SerializeField] private float gravity = -19.62f;
        [SerializeField] private float jumpHeight = 1.5f;

        [Header("Explorer Class - Grappling Hook")]
        [SerializeField] private float grappleRange = 25.0f;
        [SerializeField] private float grappleSpeed = 20.0f;
        [SerializeField] private LayerMask grappleLayerMask;
        private bool isGrappling = false;
        private Vector3 grappleTarget;

        [Header("Camera & Orientation")]
        [SerializeField] private Transform cameraTransform;

        // Internal State
        private CharacterController characterController;
        private Vector3 velocity;
        private bool isGrounded;
        private float currentSpeedMultiplier = 1.0f;

        // Properties
        public CharacterClass SelectedClass => selectedClass;
        public bool IsGrappling => isGrappling;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            ApplyClassBaseStats();

            ReactorManager reactor = GameManager.Instance != null ? GameManager.Instance.GetReactorForCity(cityID) : FindObjectOfType<ReactorManager>();
            if (reactor != null)
            {
                reactor.OnPlayerMovementPenaltyChanged.AddListener(OnMovementPenaltyChanged);
                OnMovementPenaltyChanged(reactor.IsMovementSlowed);
            }
        }

        /// <summary>
        /// Applies initial movement stats according to class specification in GDD.
        /// </summary>
        private void ApplyClassBaseStats()
        {
            switch (selectedClass)
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

            if (isGrappling)
            {
                ExecuteGrappleMovement();
            }
            else
            {
                HandleLocomotion();
                HandleJump();
                HandleGrappleInput();
            }

            ApplyGravity();
        }

        private void HandleGroundCheck()
        {
            isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small grounding force
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

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

                bool isSprinting = Input.GetKey(KeyCode.LeftShift);
                float speed = (isSprinting ? sprintSpeed : walkSpeed) * currentSpeedMultiplier;

                characterController.Move(moveDirection.normalized * speed * Time.deltaTime);
            }
        }

        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        private void HandleGrappleInput()
        {
            if (selectedClass != CharacterClass.Explorer) return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                Ray cameraRay = cameraTransform ? new Ray(cameraTransform.position, cameraTransform.forward) : new Ray(transform.position, transform.forward);

                if (Physics.Raycast(cameraRay, out RaycastHit hit, grappleRange, grappleLayerMask))
                {
                    grappleTarget = hit.point;
                    isGrappling = true;
                    Debug.Log($"[PlayerCharacterController] Grapple attached to {hit.point}");
                }
            }
        }

        private void ExecuteGrappleMovement()
        {
            Vector3 directionToTarget = (grappleTarget - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, grappleTarget);

            characterController.Move(directionToTarget * grappleSpeed * Time.deltaTime);

            if (distanceToTarget < 1.5f || Input.GetKeyDown(KeyCode.Space))
            {
                isGrappling = false;
                velocity.y = Mathf.Sqrt(jumpHeight * 1.2f * -2f * gravity); // Boost upward upon release
                Debug.Log("[PlayerCharacterController] Grapple released.");
            }
        }

        private void ApplyGravity()
        {
            if (!isGrappling)
            {
                velocity.y += gravity * Time.deltaTime;
                characterController.Move(velocity * Time.deltaTime);
            }
        }

        public void OnMovementPenaltyChanged(bool isFrozen)
        {
            currentSpeedMultiplier = isFrozen ? 0.80f : 1.0f;
            Debug.Log($"[PlayerCharacterController] Movement Speed Multiplier updated to {currentSpeedMultiplier * 100}% due to freezing status ({isFrozen}).");
        }
    }
}
