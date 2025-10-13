using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Manages mobile controls including joysticks and action buttons.
    /// Similar to PUBG Mobile control system.
    /// </summary>
    public class MobileControlsManager : MonoBehaviour
    {
        [Header("Joysticks")]
        [SerializeField] private MobileJoystick movementJoystick;
        [SerializeField] private MobileJoystick aimJoystick;
        
        [Header("Action Buttons")]
        [SerializeField] private Button jumpButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button specialButton;
        [SerializeField] private Button interactButton;
        
        [Header("Settings")]
        [SerializeField] private bool enableMobileControls = true;
        
        public Vector2 MovementInput => movementJoystick != null ? movementJoystick.InputVector : Vector2.zero;
        public Vector2 AimInput => aimJoystick != null ? aimJoystick.InputVector : Vector2.zero;
        
        private void Start()
        {
            // Check if we're on mobile platform
            #if !UNITY_ANDROID && !UNITY_IOS
            enableMobileControls = false;
            #endif
            
            if (!enableMobileControls)
            {
                gameObject.SetActive(false);
                return;
            }
            
            SetupActionButtons();
        }
        
        private void SetupActionButtons()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(OnJumpPressed);
            }
            
            if (attackButton != null)
            {
                attackButton.onClick.AddListener(OnAttackPressed);
            }
            
            if (specialButton != null)
            {
                specialButton.onClick.AddListener(OnSpecialPressed);
            }
            
            if (interactButton != null)
            {
                interactButton.onClick.AddListener(OnInteractPressed);
            }
        }
        
        private void OnJumpPressed()
        {
            Debug.Log("[MobileControls] Jump pressed");
            // Trigger jump action
        }
        
        private void OnAttackPressed()
        {
            Debug.Log("[MobileControls] Attack pressed");
            // Trigger attack action
        }
        
        private void OnSpecialPressed()
        {
            Debug.Log("[MobileControls] Special pressed");
            // Trigger special action
        }
        
        private void OnInteractPressed()
        {
            Debug.Log("[MobileControls] Interact pressed");
            // Trigger interact action
        }
        
        public void ShowControls()
        {
            gameObject.SetActive(true);
        }
        
        public void HideControls()
        {
            gameObject.SetActive(false);
        }
        
        public void ResetControls()
        {
            if (movementJoystick != null)
            {
                movementJoystick.ResetJoystick();
            }
            
            if (aimJoystick != null)
            {
                aimJoystick.ResetJoystick();
            }
        }
    }
}
