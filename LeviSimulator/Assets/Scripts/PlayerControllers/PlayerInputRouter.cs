using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayerControllers.Grapple;
using PlayerControllers.PlayerCharacterStatusStrategy;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utilities;

namespace PlayerControllers
{
    public class PlayerInputRouter:Singleton<PlayerInputRouter>
    {
        [SerializeField][LabelText("玩家移动组件")]private PlayerMovementController _movementController;
        [SerializeField][LabelText("玩家镜头组件")]private PlayerCameraController _cameraController;
        [SerializeField][LabelText("玩家钩爪组件")]private PlayerGrappleController _playerGrappleController;
        [SerializeField][LabelText("玩家滑墙组件")]private PlayerWallRunController _playerWallRunController;
        [SerializeField]private PlayerInput playerInput;
        [SerializeField][LabelText("玩家状态策略")][ReadOnly]private string _statusStrategyName;
        private BaseStatusStrategy _statusStrategy;
        private Dictionary<Type, BaseStatusStrategy> _typeCharacterStatusDic;// 类型到状态策略的映射字典
        public PlayerMovementController MovementController => _movementController;
        public PlayerCameraController CameraController => _cameraController;
        public PlayerGrappleController PlayerGrappleController => _playerGrappleController;
        public PlayerWallRunController PlayerWallRunController => _playerWallRunController;
        public PlayerInput PlayerInput => playerInput;
        public BaseStatusStrategy StatusStrategy => _statusStrategy;
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

            InitPlayerStatusFsm();
            LogUtil.Log("PlayerInputRouter初始化成功", false);
            
        }

        private void InitPlayerStatusFsm()
        {
            _typeCharacterStatusDic= new Dictionary<Type, BaseStatusStrategy>();
            var typesList= Assembly.GetAssembly(typeof(BaseStatusStrategy))  
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BaseStatusStrategy)) && !t.IsAbstract)
                .ToList();
            foreach (var type in typesList)
            {
                var instance = Activator.CreateInstance(type) as BaseStatusStrategy;
                if (instance != null)
                {
                    _typeCharacterStatusDic.Add(type, instance);
                }
            }

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
            _statusStrategy = new WalkingStatusStrategy();
            _statusStrategy.OnEnter(this);
            
            Cursor.lockState = CursorLockMode.Locked;  
            Cursor.visible = false;            // 锁定鼠标光标并隐藏
        }
        private void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case "Move":
                    HandleMoveInput(obj);
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
        }
        /// <summary>
        ///  更换玩家状态策略
        /// </summary>
        public void ChangeStatus<T>()where T:IPlayerCharacterStatusStrategy
        {
            var newStatusStrategyInstance = _typeCharacterStatusDic[typeof(T)];
            if (newStatusStrategyInstance == null)
            {
                LogUtil.LogError("新的状态策略不能为空", true);
                return;
            }
                
            _statusStrategy.OnExit();
            _statusStrategy = newStatusStrategyInstance;
            _statusStrategy.OnEnter(this);
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