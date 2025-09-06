using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;
using PlayerControllers.Refactored.Systems;
using PlayerControllers.Refactored.States;
using Utilities;

namespace PlayerControllers.Refactored
{
    /// <summary>
    /// 重构后的玩家控制器 - 主协调器
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Title("配置")]
        [SerializeField][InlineEditor] private PlayerMovementConfig movementConfig;
        [Title("组件引用")]
        [SerializeField] private Animator playerAnimator;
        
        [Title("调试信息")]
        [SerializeField, ReadOnly] private string currentState;
        [SerializeField, ReadOnly] private float currentSpeed;
        [SerializeField, ReadOnly] private bool isGrounded;
        [SerializeField, ReadOnly] private Vector3 velocity;
        [SerializeField, ReadOnly] private Vector2 moveInput;
        [SerializeField,ReadOnly] private string currentPhysicsMaterial;
    
        
        // 系统组件
        private List<IPlayerSystem> _systems = new List<IPlayerSystem>();
        private PlayerInputSystem _inputSystem;
        private PlayerMovementSystem _movementSystem;
        private PlayerCameraSystem _cameraSystem;
        private PlayerWallRunSystem _wallRunSystem;
        
        // 核心组件
        private PlayerStateMachine _stateMachine;
        private PlayerRuntimeData _runtimeData;
        
        // 状态实例
        private Dictionary<PlayerState, IState> _states = new Dictionary<PlayerState, IState>();
        
        // 公共访问器
        public PlayerRuntimeData RuntimeData => _runtimeData;
        public PlayerStateMachine StateMachine => _stateMachine;
        public PlayerMovementSystem MovementSystem => _movementSystem;
        public PlayerCameraSystem CameraSystem => _cameraSystem;
        public PlayerInputSystem InputSystem => _inputSystem;
        public PlayerWallRunSystem WallRunSystem => _wallRunSystem;
        public Animator PlayerAnimator => playerAnimator;

        private void Awake()
        {
            InitializeSceneObjects();
            InitializeCore();
            InitializeSystems();
            InitializeStates();
            SubscribeToEvents();
        }

        /// <summary>
        ///  初始化场景对象引用（如需要）
        /// </summary>
        private void InitializeSceneObjects()
        {
            
            if (playerAnimator == null)
            {
                playerAnimator=GetComponentInChildren<Animator>();
                if (playerAnimator == null)
                {
                    LogUtil.LogWarning("PlayerAnimator未分配且在子对象中未找到Animator组件");
                }
            }
        }

        private void Start()
        {
            // 启动状态机
            _stateMachine.Initialize(PlayerState.Idle);
        }
        
        private void Update()
        {
            // 更新所有系统
            foreach (var system in _systems)
            {
                if (system.IsEnabled)
                    system.Update();
            }
            
            // 更新状态机
            _stateMachine.Update();
            
            // 更新调试信息
            UpdateDebugInfo();
        }
        
        private void FixedUpdate()
        {
            // 固定更新所有系统
            foreach (var system in _systems)
            {
                if (system.IsEnabled)
                    system.FixedUpdate();
            }
            
            // 固定更新状态机
            _stateMachine.FixedUpdate();
            
            // 根据地面状态更改物理材质
            ChangePhysicsMaterial(_runtimeData.IsGrounded ? 
                Enums.PlayerPhysicsMaterialType.OnGround : Enums.PlayerPhysicsMaterialType.InAir);
        }
        
        private void InitializeCore()
        {
            // 初始化运行时数据
            _runtimeData = new PlayerRuntimeData();
            
            // 初始化状态机
            _stateMachine = new PlayerStateMachine(this);
        }
        
        private void InitializeSystems()
        {
            // 获取或添加系统组件
            _inputSystem = GetComponent<PlayerInputSystem>();
            if (_inputSystem == null)
                _inputSystem = gameObject.AddComponent<PlayerInputSystem>();
                
            _movementSystem = GetComponent<PlayerMovementSystem>();
            if (_movementSystem == null)
                _movementSystem = gameObject.AddComponent<PlayerMovementSystem>();
                
            _cameraSystem = GetComponentInChildren<PlayerCameraSystem>();
            if (_cameraSystem == null)
            {
                // 查找摄像机
                Camera playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    _cameraSystem = playerCamera.gameObject.AddComponent<PlayerCameraSystem>();
                }
            }
            
            _wallRunSystem = GetComponent<PlayerWallRunSystem>();
            if (_wallRunSystem == null)
                _wallRunSystem = gameObject.AddComponent<PlayerWallRunSystem>();
            
            // 添加系统到列表
            _systems.Add(_inputSystem);
            _systems.Add(_movementSystem);
            _systems.Add(_wallRunSystem);
            if (_cameraSystem != null)
                _systems.Add(_cameraSystem);
            
            // 初始化所有系统
            foreach (var system in _systems)
            {
                system.Initialize(this,movementConfig);
            }
        }
        
        private void InitializeStates()
        {
            
            // 创建所有状态实例
            _states[PlayerState.Idle] = new IdleState(this);
            _states[PlayerState.Walking] = new WalkingState(this);
            _states[PlayerState.Running] = new RunningState(this);
            _states[PlayerState.Jumping] = new JumpingState(this);
            _states[PlayerState.Falling] = new FallingState(this);
            _states[PlayerState.Crouching] = new CrouchingState(this);
            _states[PlayerState.Sliding] = new SlidingState(this);
            _states[PlayerState.WallRunning] = new WallRunningState(this);
            
            // 注册状态到状态机
            foreach (var kvp in _states)
            {
                _stateMachine.RegisterState(kvp.Key, kvp.Value);
            }
        }
        
        private void SubscribeToEvents()
        {
            // 订阅输入事件
            PlayerInputEvents.OnJumpPressed += HandleJumpPressed;
            PlayerInputEvents.OnCrouchPressed += HandleCrouchPressed;
            PlayerInputEvents.OnCrouchReleased += HandleCrouchReleased;
        }
        
        private void HandleJumpPressed()
        {
            // 只有在地面上才能跳跃
            if (_runtimeData.IsGrounded && _stateMachine.CurrentState != PlayerState.Crouching)
            {
                _stateMachine.ChangeState(PlayerState.Jumping);
            }
        }
        
        private void HandleCrouchPressed()
        {
            _runtimeData.SetCrouching(true);
        }
        
        private void HandleCrouchReleased()
        {
            _runtimeData.SetCrouching(false);
        }
        
        private void UpdateDebugInfo()
        {
            currentState = _stateMachine.CurrentState.ToString();
            currentSpeed = _runtimeData.Speed;
            isGrounded = _runtimeData.IsGrounded;
            velocity = _runtimeData.Velocity;
            moveInput = _runtimeData.MoveInput;
        }
        
        /// <summary>
        /// 启用/禁用特定系统
        /// </summary>
        public void SetSystemEnabled<T>(bool enabled) where T : IPlayerSystem
        {
            foreach (var system in _systems)
            {
                if (system is T)
                {
                    system.IsEnabled = enabled;
                    break;
                }
            }
        }
        
        /// <summary>
        /// 获取特定系统
        /// </summary>
        public T GetSystem<T>() where T : IPlayerSystem
        {
            foreach (var system in _systems)
            {
                if (system is T)
                    return (T)system;
            }
            return default(T);
        }
        
        /// <summary>
        /// 强制改变状态（用于外部系统，如钩爪）
        /// </summary>
        public void ForceChangeState(PlayerState newState)
        {
            _stateMachine.ChangeState(newState);
        }
        
        /// <summary>
        /// 重置玩家到初始状态
        /// </summary>
        [Button]
        public void ResetPlayer()
        {
            // 重置物理
            if (_movementSystem != null && _movementSystem.Rigidbody != null)
            {
                _movementSystem.Rigidbody.linearVelocity = Vector3.zero;
                _movementSystem.Rigidbody.angularVelocity = Vector3.zero;
            }
            
            // 重置摄像机
            _cameraSystem?.ResetRotation();
            
            // 重置状态
            _stateMachine.ChangeState(PlayerState.Idle);
            
            Debug.Log("Player reset complete");
        }
        
        private void OnDestroy()
        {
            // 清理事件订阅
            PlayerInputEvents.OnJumpPressed -= HandleJumpPressed;
            PlayerInputEvents.OnCrouchPressed -= HandleCrouchPressed;
            PlayerInputEvents.OnCrouchReleased -= HandleCrouchReleased;
            
            // 清理所有系统
            foreach (var system in _systems)
            {
                system.Cleanup();
            }
            
            // 清理状态机
            _stateMachine?.Cleanup();
        }
        
        private void OnValidate()
        {
            // 确保配置文件已分配
            if (movementConfig == null)
            {
                Debug.LogWarning("PlayerMovementConfig is not assigned!");
            }
        }
        /// <summary>
        ///  更改物理材质
        /// </summary>
        /// <param name="materialType"></param>
        private void ChangePhysicsMaterial(Enums.PlayerPhysicsMaterialType materialType)
        {
            var playerCollider = _movementSystem.PlayerCollider;
            if (playerCollider == null)
            {
                LogUtil.LogError("PlayerCollider不存在,试图更改物理材质失败");
                return;
            }
            var material = materialType switch
            {
                Enums.PlayerPhysicsMaterialType.InAir => movementConfig.PlayerInAirMaterial,
                Enums.PlayerPhysicsMaterialType.OnGround => movementConfig.PlayerOnGroundMaterial,
                _ => null
            };
            if (material == null)
            {
                LogUtil.LogError($"未为{materialType}分配物理材质或者是玩家config文件未分配");
                return;
            }
            if(currentPhysicsMaterial == material.name)
            {
                
                return;
            }
            playerCollider.material = material;
            currentPhysicsMaterial = material.name;
        }
    }
}
