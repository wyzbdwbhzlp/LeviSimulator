using System.Collections.Generic;
using GlobalGameManager;
using HUD;
using UnityEngine;
using Sirenix.OdinInspector;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;
using PlayerControllers.Refactored.Systems;
using PlayerControllers.Refactored.States;
using SettingPanel;
using UIManager;
using Utilities;

namespace PlayerControllers.Refactored
{
    /// <summary>
    /// 重构后的玩家控制器 - 主协调器
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Title("配置")]
        [SerializeField][InlineEditor]private PlayerMovementConfig movementConfig;
        [SerializeField][InlineEditor]private PlayerGrapplingConfig grapplingConfig;
        [SerializeField][InlineEditor]private PlayerSkillConfig skillConfig;
        [Title("组件引用")]
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private Transform grapplingMuzzle;
        
        [Title("调试信息")]
        [SerializeField, ReadOnly] private string currentState;
        [SerializeField, ReadOnly] private float currentSpeed;
        [SerializeField, ReadOnly] private bool isGrounded;
        [SerializeField, ReadOnly] private Vector3 velocity;
        [SerializeField, ReadOnly] private Vector2 moveInput;
        [SerializeField,ReadOnly] private string currentPhysicsMaterial;
        [SerializeField, ReadOnly] private bool isOnSlope;
        [SerializeField, ReadOnly] private float slopeRotationZ;
        [SerializeField, ReadOnly] private Vector3 slopeDirection;
        private static bool _isCursorLocked = true;
    
        
        // 系统组件
        private List<IPlayerSystem> _systems = new List<IPlayerSystem>();
        private PlayerInputSystem _inputSystem;
        private PlayerMovementSystem _movementSystem;
        private PlayerCameraSystem _cameraSystem;
        private PlayerWallRunSystem _wallRunSystem;
        private PlayerGrapplingSystem _grapplingSystem;
        private PlayerSkillSystem _skillSystem;
        
        // 核心组件
        private PlayerStateMachine _stateMachine;
        private PlayerRuntimeData _runtimeData;
        
        // 状态实例
        private Dictionary<PlayerState, IState> _states = new Dictionary<PlayerState, IState>();
        
        private Coroutine _playerRestartCoroutine;
        
        // 公共访问器
        public PlayerRuntimeData RuntimeData => _runtimeData;
        public PlayerStateMachine StateMachine => _stateMachine;
        public PlayerMovementSystem MovementSystem => _movementSystem;
        public PlayerGrapplingConfig GrapplingConfig => grapplingConfig;
        public PlayerCameraSystem CameraSystem => _cameraSystem;
        public PlayerInputSystem InputSystem => _inputSystem;
        public PlayerWallRunSystem WallRunSystem => _wallRunSystem;
        public Animator PlayerAnimator => playerAnimator;
        public Transform GrapplingMuzzle => grapplingMuzzle;
        /// <summary>
        /// 获取当前光标是否被锁定
        /// </summary>
        public bool IsCursorLocked => _isCursorLocked;

        
        public void Initialize()
        {
            InitializeSceneObjects();
            InitializeCore();
            InitializeSystems();
            InitializeStates();
            SubscribeToEvents();
            
            _stateMachine.Initialize(PlayerState.Idle);

            foreach (var system in _systems)
            {
                system.IsEnabled=true;
            }
        }

        public bool IsSystemAllInitialized()
        {
            foreach (var system in _systems)
            {
                if (!system.IsInitialized)
                    return false;
            }
            return true;
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
            
            //重赋值Config
            LoadSettingFromSetting();
            
        }

        private void LoadSettingFromSetting()
        {
            if(movementConfig==null)
                return;
            movementConfig.SetMouseSensitivity = SettingController.Instance.CurrentSettings.MouseSensitivity;
            movementConfig.SetFOV = SettingController.Instance.CurrentSettings.FieldOfView;

        }

        private void InitializeSystems()
        {
            // 获取或添加系统组件
            _inputSystem = GetComponent<PlayerInputSystem>();
            if (_inputSystem == null)
            {
                _inputSystem = gameObject.AddComponent<PlayerInputSystem>();
                LogUtil.LogWarning("PlayerInputSystem组件未找到，已自动添加，请确保已正确配置Input Actions");
            }

            _movementSystem = GetComponent<PlayerMovementSystem>();
            if (_movementSystem == null)
            {
                _movementSystem = gameObject.AddComponent<PlayerMovementSystem>();
                LogUtil.LogWarning("PlayerMovementSystem组件未找到，已自动添加");
            }

            _cameraSystem = GetComponentInChildren<PlayerCameraSystem>();
            if (_cameraSystem == null)
            {
                // 查找摄像机
                Camera playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    _cameraSystem = playerCamera.gameObject.AddComponent<PlayerCameraSystem>();
                    LogUtil.LogWarning("PlayerCameraSystem组件未找到，已在摄像机对象上自动添加");
                }
            }
            
            _wallRunSystem = GetComponent<PlayerWallRunSystem>();
            if (_wallRunSystem == null)
            {
                _wallRunSystem = gameObject.AddComponent<PlayerWallRunSystem>();
                LogUtil.LogWarning("PlayerWallRunSystem组件未找到，已自动添加");
            }

            _grapplingSystem = GetComponent<PlayerGrapplingSystem>();
            if (_grapplingSystem == null)
            {
                _grapplingSystem = gameObject.AddComponent<PlayerGrapplingSystem>();
                LogUtil.LogWarning("PlayerGrapplingSystem组件未找到，已自动添加");
            }

            _grapplingSystem.SetGrappleSetting(grapplingConfig,grapplingMuzzle);
            
            _skillSystem = GetComponent<PlayerSkillSystem>();
            if (_skillSystem == null)
            {
                _skillSystem = gameObject.AddComponent<PlayerSkillSystem>();
                LogUtil.LogWarning("PlayerSkillSystem组件未找到，已自动添加");
            }

            // 添加系统到列表
            _systems.Add(_inputSystem);
            _systems.Add(_movementSystem);
            _systems.Add(_wallRunSystem);
            _systems.Add(_grapplingSystem);
            _systems.Add(_skillSystem);
            if (_cameraSystem != null)
                _systems.Add(_cameraSystem);
            
            // 初始化所有系统
            _inputSystem.Initialize(this,movementConfig);
            _movementSystem.Initialize(this,movementConfig);
            _cameraSystem?.Initialize(this,movementConfig);
            _wallRunSystem.Initialize(this,movementConfig);
            _grapplingSystem.Initialize(this,grapplingConfig);
            _skillSystem.Initialize(this,skillConfig);
        }
        
        private void InitializeStates()
        {
            //创建所有状态实例
            _states[PlayerState.Idle] = new IdleState(this);
            _states[PlayerState.Walking] = new WalkingState(this);
            _states[PlayerState.Running] = new RunningState(this);
            _states[PlayerState.Jumping] = new JumpingState(this);
            _states[PlayerState.Falling] = new FallingState(this);
            _states[PlayerState.Crouching] = new CrouchingState(this);
            _states[PlayerState.Sliding] = new SlidingState(this);
            _states[PlayerState.WallRunning] = new WallRunningState(this);
            _states[PlayerState.Dashing] = new DashState(this);
            
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
            PlayerInputEvents.OnRestartFromCheckpointPressed += HandleRestartFromCheckpointPressed;
            PlayerInputEvents.OnRestartFromCheckpointReleased += HandleRestartFromCheckpointReleased;
            
            //订阅场景事件（如有）
        }
        /// <summary>
        /// 锁定并隐藏光标（启用摄像机输入）
        /// </summary>
        public static void LockAndHideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            MainUIManager.ShowHUDComponent<CrosshairHUD>();
            GlobalManager.Instance.playerSpawnManager.GetCurrentPlayer().GetComponent<PlayerController>().GetSystem<PlayerInputSystem>().IsEnabled=true;
            _isCursorLocked = true;
        }
        /// <summary>
        /// 解锁并显示光标（禁用摄像机输入）
        /// </summary>
        public static void UnlockAndShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            MainUIManager.HideHUDComponent<CrosshairHUD>();
            GlobalManager.Instance.playerSpawnManager.GetCurrentPlayer().GetComponent<PlayerController>().GetSystem<PlayerInputSystem>().IsEnabled=false;
            _isCursorLocked = false;
        }

        private void HandleRestartFromCheckpointReleased()
        {
            LogUtil.Log( "玩家取消从存档点重生");
            if (_playerRestartCoroutine != null)
            {
                StopCoroutine(_playerRestartCoroutine);
                _playerRestartCoroutine = null;
            }
        }

        private void HandleRestartFromCheckpointPressed()
        {
            LogUtil.Log( "玩家请求从存档点重生");
            if (_playerRestartCoroutine == null)
            {
                _playerRestartCoroutine = StartCoroutine(WaitAndRestartPlayer(2f));
            }
        }
        private IEnumerator<WaitForSeconds> WaitAndRestartPlayer(float delay)
        {
            yield return new WaitForSeconds(delay);
            GlobalManager.Instance.playerSpawnManager.PlayerIsDeath();
            _playerRestartCoroutine = null;
        }

        private void HandleJumpPressed()
        {
            _stateMachine.CurrentStateInstance.OnJumpPressed();
            // // 只有在地面上才能跳跃
            // if (_runtimeData.IsGrounded && _stateMachine.CurrentState != PlayerState.Crouching)
            // {
            //     _stateMachine.ChangeState(PlayerState.Jumping);
            // }
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
            isOnSlope = _runtimeData.IsOnSlope;
            slopeRotationZ = _runtimeData.SlopeRotationZ;
            slopeDirection = _runtimeData.SlopeDirection;
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
        private void ResetPlayer()
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
        private void OnDisable()
        {
            UnsubscribeToEvents();// 取消订阅所有事件
        }
        private void OnDestroy()
        {
            // 清理所有系统
            foreach (var system in _systems)
            {
                system.CleanUp();
                Destroy(system as MonoBehaviour);
            }
            
            // 清理状态机
            _stateMachine?.Cleanup();
            _runtimeData = null;
        }

        private void UnsubscribeToEvents()
        {
            PlayerInputEvents.OnJumpPressed -= HandleJumpPressed;
            PlayerInputEvents.OnCrouchPressed -= HandleCrouchPressed;
            PlayerInputEvents.OnCrouchReleased -= HandleCrouchReleased;
            PlayerInputEvents.OnRestartFromCheckpointPressed -= HandleRestartFromCheckpointPressed;
            PlayerInputEvents.OnRestartFromCheckpointReleased -= HandleRestartFromCheckpointReleased;
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

        public void SetPlayerPostion(Vector3 position)
        {
            transform.position = position;
        }

    }
}
