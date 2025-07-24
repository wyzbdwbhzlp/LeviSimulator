using System;
using PlayerControllers.Grapple;
using PlayerControllers.PlayerCharacterStatusStrategy;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Utilities;

namespace PlayerControllers
{
    public class PlayerInputRouter:Singleton<PlayerInputRouter>
    {
        [SerializeField]private PlayerMovementController _movementController;
        [SerializeField]private PlayerCameraController _cameraController;
        [SerializeField]private PlayerGrappleManager _playerGrappleController;
        [SerializeField]private PlayerInput playerInput;
        private IPlayerCharacterStatusStrategy _statusStrategy;
        public PlayerMovementController MovementController => _movementController;
        public PlayerCameraController CameraController => _cameraController;
        public PlayerGrappleManager PlayerGrappleController => _playerGrappleController;
        public PlayerInput PlayerInput => playerInput;
        protected override void Awake()
        {
            base.Awake();
            if (_movementController == null)
                _movementController = GetComponentInChildren<PlayerMovementController>();
            if (_cameraController == null)
                _cameraController= GetComponentInChildren<PlayerCameraController>();
            if (_playerGrappleController == null)
                _playerGrappleController= GetComponentInChildren<PlayerGrappleManager>();
            
            _movementController.SetRouter(this);
            _cameraController.SetRouter(this);
            _playerGrappleController.SetRouter(this);
            
        }

        public void Start()
        {
            if (_movementController == null || _cameraController == null || _playerGrappleController == null)
            {
                LogUtil.LogError("PlayerInputRouter未正确获取到子模块");
            }
            _statusStrategy = new WalkingStatusStrategyStrategy();
            _statusStrategy.OnEnter(this);
        }

        public void Update()
        {
            _statusStrategy.LogicUpdate();
        }
        /// <summary>
        ///  更换玩家状态策略
        /// </summary>
        public void ChangeStatus(IPlayerCharacterStatusStrategy newStatusStrategy)
        {
            if (newStatusStrategy == null)
            {
                LogUtil.LogError("新的状态策略不能为空", true);
                return;
            }
                
            _statusStrategy.OnExit();
            _statusStrategy = newStatusStrategy;
            _statusStrategy.OnEnter(this);
        }
    }
}