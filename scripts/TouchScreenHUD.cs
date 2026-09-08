using UnityEngine;
using UnityEngine.UI;

namespace EcoDeLasCenizas.UI
{
    /// <summary>
    /// Mobile touch UI HUD managing virtual movement joystick and touch buttons
    /// for jumping, interacting, and triggering class abilities.
    /// Automatically activates on mobile platforms (Application.isMobilePlatform).
    /// </summary>
    public class TouchScreenHUD : MonoBehaviour
    {
        public static TouchScreenHUD Instance { get; private set; }

        [Header("Touch HUD Root Canvas")]
        [SerializeField] private GameObject mobileHudCanvasGroup;

        [Header("Virtual Joystick Elements")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;

        [Header("Action Touch Buttons")]
        [SerializeField] private Button jumpButton;
        [SerializeField] private Button interactButton;
        [SerializeField] private Button abilityButton;

        // Joystick Input Values
        public Vector2 JoystickInput { get; private set; }
        public bool IsJumpPressed { get; private set; }
        public bool IsInteractPressed { get; private set; }
        public bool IsAbilityPressed { get; private set; }

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
            // Auto-enable touch controls on Android/iOS/Mobile or in Editor if forced
            bool enableMobileUI = Application.isMobilePlatform || Application.platform == RuntimePlatform.Android;

            if (mobileHudCanvasGroup != null)
            {
                mobileHudCanvasGroup.SetActive(enableMobileUI);
            }

            if (jumpButton != null) jumpButton.onClick.AddListener(OnJumpButtonClicked);
            if (interactButton != null) interactButton.onClick.AddListener(OnInteractButtonClicked);
            if (abilityButton != null) abilityButton.onClick.AddListener(OnAbilityButtonClicked);
        }

        public void UpdateJoystickInput(Vector2 input)
        {
            JoystickInput = Vector2.ClampMagnitude(input, 1.0f);
        }

        private void OnJumpButtonClicked()
        {
            IsJumpPressed = true;
            Invoke(nameof(ResetJumpFlag), 0.1f);
        }

        private void OnInteractButtonClicked()
        {
            IsInteractPressed = true;
            Invoke(nameof(ResetInteractFlag), 0.1f);
        }

        private void OnAbilityButtonClicked()
        {
            IsAbilityPressed = true;
            Invoke(nameof(ResetAbilityFlag), 0.1f);
        }

        private void ResetJumpFlag() => IsJumpPressed = false;
        private void ResetInteractFlag() => IsInteractPressed = false;
        private void ResetAbilityFlag() => IsAbilityPressed = false;
    }
}
