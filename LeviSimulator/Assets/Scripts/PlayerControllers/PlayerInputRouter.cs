using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayerControllers.Grapple;
using PlayerControllers.PlayerCharacterStatusStrategy;
using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utilities;

namespace PlayerControllers
{
    [Obsolete]
    public class PlayerInputRouter:Singleton<PlayerInputRouter>
    {
        [SerializeField][LabelText("玩家移动组件")]private PlayerMovementController _movementController;
        [SerializeField][LabelText("玩家镜头组件")]private PlayerCameraController _cameraController;
        [SerializeField][LabelText("玩家钩爪组件")]private PlayerGrappleController _playerGrappleController;
        [SerializeField][LabelText("玩家滑墙组件")]private PlayerWallRunController _playerWallRunController;
        [SerializeField]private PlayerInput playerInput;
        [SerializeField][LabelText("玩家状态策略")][ReadOnly]private string _statusStrategyName;
        [SerializeField][LabelText("状态子类")][ReadOnly]private string _subStatusStrategyName;
        [SerializeField][ReadOnly]private bool _isHoldingJump = false;
        [SerializeField][ReadOnly]private bool _isHoldingCrouch = false;
        
        private HierarchicalBaseState _statusStrategy;
        public PlayerMovementController MovementController => _movementController;
        public PlayerCameraController CameraController => _cameraController;
        public PlayerGrappleController PlayerGrappleController => _playerGrappleController;
        public PlayerWallRunController PlayerWallRunController => _playerWallRunController;
        public PlayerInput PlayerInput => playerInput;
        public HierarchicalBaseState StatusStrategy => _statusStrategy;
        public bool IsHoldingJump=> _isHoldingJump;
        public bool IsHoldingCrouch => _isHoldingCrouch;
        protected override void Awake()
        {
            base.Awake();
            if (_movementController == null)
                _movementController = GetComponentInChildren<PlayerMovementController>();
            if (_cameraController == null)
                _cameraController= GetComponentInChildren<PlayerCameraController>();
            if (_playerGrappleController == null)
                _playerGrappleController= GetComponentInChildren<PlayerGrappleController>();
            if (_playerWallRunController == null)
                _playerWallRunController = GetComponentInChildren<PlayerWallRunController>();
            
            if (_movementController == null || _cameraController == null || _playerGrappleController == null|| _playerWallRunController == null)
            {
                LogUtil.LogError("PlayerInputRouter未正确获取到子模块");
                return;
            }
            
            _movementController.SetRouter(this);
            _cameraController.SetRouter(this);
            _playerGrappleController.SetRouter(this);
            _playerWallRunController.SetRouter(this);

            PlayerCharacterStatusStrategyFactory.InitializeStates(); //todo 应该在游戏开始时预加载
            
            LogUtil.Log("PlayerInputRouter初始化成功", false);
            
        }
        

        protected void OnEnable()
        {
            EventBroadcaster.EnablePlayerRbGravity+= EnablePlayerRbGravity;
            EventBroadcaster.DisablePlayerRbGravity+= DisablePlayerRbGravity;
            EventBroadcaster.EchoViewUIOpened+=OnEchoViewUIOpened;
            EventBroadcaster.EchoViewUIClosed+=OnEchoViewUIClosed;
            PlayerInput.onActionTriggered+= HandleonActionTriggered;
        }
        


        protected void OnDisable()
        {
            EventBroadcaster.EnablePlayerRbGravity -= EnablePlayerRbGravity;
            EventBroadcaster.DisablePlayerRbGravity -= DisablePlayerRbGravity;
            EventBroadcaster.EchoViewUIOpened -= OnEchoViewUIOpened;
            EventBroadcaster.EchoViewUIClosed -= OnEchoViewUIClosed;
            PlayerInput.onActionTriggered-= HandleonActionTriggered;
        }
        

        public void Start()
        {
            _statusStrategy = new GroundedState();
            _statusStrategy.OnEnter(this);
            
            Cursor.lockState = CursorLockMode.Locked;  
            Cursor.visible = false;            // 锁定鼠标光标并隐藏
        }
        private void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            _statusStrategy.HandleonActionTriggered(obj);
            switch (obj.action.name)
            {
                case "Move":
                    HandleMoveInput(obj);
                    break;
                case "Jump":
                    HandleJumpInput(obj);
                    break;
                case "Crouch":
                    HandleCrouchInput(obj);
                    break;
            }
        }

        private void HandleCrouchInput(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    _isHoldingCrouch = true;
                    break;
                case InputActionPhase.Canceled:
                    _isHoldingCrouch = false;
                    break;
            }
        }

        private void HandleJumpInput(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    _isHoldingJump = true;
                    break;
                case InputActionPhase.Performed:
                    _isHoldingJump =true;
                    break;
                default:
                    _isHoldingJump = false;
                    break;
            }
        }

        private void HandleMoveInput(InputAction.CallbackContext callbackContext)
        {
            var moveInput= callbackContext.ReadValue<Vector2>();
            _movementController.ApplyMovement(moveInput);
        }

        public void Update()
        {
            _statusStrategy.LogicUpdate();
            _statusStrategyName= _statusStrategy.GetType().Name;
            _subStatusStrategyName= _statusStrategy.CurrentSubState?.GetType().Name ?? "无子状态";
        }
        /// <summary>
        ///  更换玩家状态策略
        /// </summary>
        public void ChangeParentStatus<T>()where T:HierarchicalBaseState
        {
            var newStatusStrategyInstance = PlayerCharacterStatusStrategyFactory.GetParentState<T>();
            if (newStatusStrategyInstance == null)
            {
                LogUtil.LogError("新的状态策略不能为空", true);
                return;
            }
            LogUtil.Log($"玩家状态策略从{_statusStrategyName}更换为{newStatusStrategyInstance.GetType().Name}");
            _statusStrategy.OnExit();
            EventBroadcaster.CallPlayerChangeStatusEvent(_statusStrategy, newStatusStrategyInstance);
            _statusStrategy = newStatusStrategyInstance;
            _statusStrategy.OnEnter(this);
            DebugSpeedShowController.Instance?.SetRouter(this); //todo debuging
        }

        public void DisablePlayerRbGravity()
        {
            _movementController.DisablePlayerRbGravity();
        }

        public void EnablePlayerRbGravity()
        {
            _movementController.EnablePlayerRbGravity();
        }
        private void OnEchoViewUIOpened()
        {
            OnUIOpen();
        }
        private void OnEchoViewUIClosed()
        {
            OnUIClose();
        }
        private void OnUIOpen()
        {
            Cursor.lockState = CursorLockMode.None;  
            Cursor.visible = true;        
            playerInput.enabled = false;
        }
        private void OnUIClose()
        {
            Cursor.lockState = CursorLockMode.Locked;  
            Cursor.visible = false;        
            playerInput.enabled = true;
        }
    }
}