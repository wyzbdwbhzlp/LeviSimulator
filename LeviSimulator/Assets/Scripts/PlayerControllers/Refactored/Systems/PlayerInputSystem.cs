using UnityEngine;
using UnityEngine.InputSystem;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;
using UnityEngine.Events;

namespace PlayerControllers.Refactored.Systems
{
    /// <summary>
    /// 输入处理系统
    /// </summary>
    public class PlayerInputSystem : MonoBehaviour, IPlayerSystem
    {
        [SerializeField] private PlayerInput playerInput;
        
        // 输入动作引用
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _crouchAction;
        private InputAction _sprintAction;
        private InputAction _grappleAction;
        private InputAction _interactAction;
        private InputAction _restartFromCheckpointAction;
        private InputAction _bulletTimeAction;
        
        private UnityAction _interactActionCallback;
        
        public bool IsEnabled { get; set; } = true;
        
        public void Initialize(PlayerController playerController,PlayerMovementConfig playerConfig)
        {
            if (playerInput == null)
                playerInput = GetComponent<PlayerInput>();
                
            SetupInputActions();
        }

        public void Update() { }

        public void FixedUpdate() { }

        public void SetupInputActions()
        {
            var actionMap = playerInput.actions;
            
            _moveAction = actionMap.FindAction("Move");
            _lookAction = actionMap.FindAction("Look");
            _jumpAction = actionMap.FindAction("Jump");
            _crouchAction = actionMap.FindAction("Crouch");
            _sprintAction = actionMap.FindAction("Sprint");
            _grappleAction = actionMap.FindAction("Grapple");
            _interactAction = actionMap.FindAction("Interact");
            _restartFromCheckpointAction= actionMap.FindAction("RestartFromCheckpoint");
            _bulletTimeAction= actionMap.FindAction("BulletTime");
            
            
            // 绑定输入事件
            _moveAction.performed += OnMovePerformed;
            _moveAction.canceled += OnMoveCanceled;
            
            _lookAction.performed += OnLookPerformed;
            
            _jumpAction.started += OnJumpStarted;
            _jumpAction.canceled += OnJumpCanceled;
            
            _crouchAction.started += OnCrouchStarted;
            _crouchAction.canceled += OnCrouchCanceled;
            
            _sprintAction.started += OnSprintStarted;
            _sprintAction.canceled += OnSprintCanceled;
            
            _grappleAction.started += OnGrappleStarted;
            _grappleAction.canceled += OnGrappleCanceled;
            
            _restartFromCheckpointAction.started += OnRestartFromCheckpointStarted;
            _restartFromCheckpointAction.canceled += OnRestartFromCheckpointCanceled;
            
            _interactAction.started += CallInteractCallback;
            _bulletTimeAction.started += OnBulletTimeStarted;
            
            
            
            EnableInput();// 启用输入
            
        }

        private void OnBulletTimeStarted(InputAction.CallbackContext obj)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerBulletTimePressed();
        }

        private void OnRestartFromCheckpointCanceled(InputAction.CallbackContext obj)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerRestartFromCheckpointReleased();
            
        }

        private void OnRestartFromCheckpointStarted(InputAction.CallbackContext obj)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerRestartFromCheckpointPressed();
        }


        private void EnableInput()
        {
            playerInput.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void DisableInput()
        {
            playerInput.enabled = false;
        }
        
        // 输入事件处理
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerMoveInput(context.ReadValue<Vector2>());
        }
        
        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerMoveInput(Vector2.zero);
        }
        
        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerLookInput(context.ReadValue<Vector2>());
        }
        
        private void OnJumpStarted(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerJumpPressed();
        }
        
        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerJumpReleased();
        }
        
        private void OnCrouchStarted(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerCrouchPressed();
        }
        
        private void OnCrouchCanceled(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerCrouchReleased();
        }
        
        private void OnSprintStarted(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerSprintPressed();
        }
        
        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerSprintReleased();
        }
        
        private void OnGrappleStarted(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerGrapplePressed();
        }
        
        private void OnGrappleCanceled(InputAction.CallbackContext context)
        {
            if (!IsEnabled) return;
            PlayerInputEvents.TriggerGrappleReleased();
        }
        public void RegisterInteractCallback(UnityAction callback)
        {
            _interactActionCallback = callback;
        }
        private void CallInteractCallback(InputAction.CallbackContext obj)
        {
            _interactActionCallback?.Invoke();
            UnregisterInteractCallback();
        }
        public void UnregisterInteractCallback()
        {
            _interactActionCallback = null;
        }
        
        public void Cleanup()
        {
            PlayerInputEvents.ClearAllEvents();
            DisableInput();
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
