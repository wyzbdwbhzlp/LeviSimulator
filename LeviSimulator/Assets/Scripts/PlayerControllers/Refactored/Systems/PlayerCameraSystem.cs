using System;
using HUD;
using UnityEngine;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;
using SettingPanel;
using Sirenix.OdinInspector;
using UIManager;
using UnityEngine.Serialization;
using Utilities;
using Random = UnityEngine.Random;

namespace PlayerControllers.Refactored.Systems
{
    /// <summary>
    /// 玩家摄像机控制系统
    /// </summary>
    public class PlayerCameraSystem : MonoBehaviour, IPlayerSystem
    {
        [Header("摄像机设置")]
        [SerializeField] private Transform playerBody;
        [SerializeField] private Camera playerCamera;
        
        [Header("FPS优化")]
        [SerializeField] private bool useRawInput = true; // 是否使用原始输入（更适合FPS）
        [SerializeField] private bool frameRateIndependent = true; // 是否启用帧率无关控制
        [Header("配置")]
        [SerializeField]private PlayerMovementConfig movementConfig;
        
        [Header("视角限制")]
        [SerializeField] private float minVerticalAngle = -90f;
        [SerializeField] private float maxVerticalAngle = 90f;
        
        [Header("特效")]
        [SerializeField] private float maxTiltAngle = 15f;
        [SerializeField] private float tiltSpeed = 5f;
        [FormerlySerializedAs("speedLineController")]
        [Header("模组")]
        [SerializeField]private CameraSpeedLineHUD speedLineHUD;
        
        private PlayerController _playerController;
        private PlayerRuntimeData _runtimeData;

        private SettingController _settingsController;

        private Coroutine _lookAssistCoroutine; // 视角辅助协程引用
        private float _targetXRotation = 0f; // 目标X旋转角度（用于视角辅助）
        private float _targetYRotation = 0f; // 目标Y旋转角度（用于视角辅助）
        
        // 旋转参数
        private float _xRotation = 0f;
        private float _yRotation = 0f;
        private float _targetTilt = 0f;
        private float _currentTilt = 0f;
        
        // 当前输入（用于即时响应）
        private Vector2 _currentLookInput;
        
        // 静态光标锁定状态（用于全局控制摄像机输入）

        

        public bool IsEnabled { get; set; } = true;
        [ShowInInspector]public bool IsInitialized { get; set; } = false;
        public Transform CameraTransform => playerCamera.transform;
        public Camera Camera => playerCamera;
        private float MouseSensitivity =>movementConfig.MouseSensitivity;
        private float Smoothness =>movementConfig.Smoothness;

        
        public void Initialize(PlayerController playerController,ScriptableObject playerConfig)
        {
            _playerController = playerController;
            movementConfig = playerConfig as PlayerMovementConfig;
            _runtimeData = playerController.RuntimeData;
            
            // 获取组件引用
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();
            if (playerBody == null)
                playerBody = playerController.transform;// 默认使用玩家物体作为身体

            _settingsController = SettingController.Instance;
            if (_settingsController == null)
            {
                Debug.LogWarning("SettingController 实例未找到，摄像机设置将无法立即响应配置变更。");
            }
            else if (movementConfig != null)
            {
                var currentSettings = _settingsController.CurrentSettings;
                movementConfig.SetMouseSensitivity = currentSettings.MouseSensitivity;
                movementConfig.SetFOV = currentSettings.FieldOfView;
                if (playerCamera != null)
                {
                    playerCamera.fieldOfView = currentSettings.FieldOfView;
                }
            }
            

            // 订阅事件
            RefreshEventSubscription(); 
            
            // 初始化旋转值
            Vector3 initialEuler = playerBody.eulerAngles;
            _yRotation = initialEuler.y;
            _xRotation = playerCamera.transform.localEulerAngles.x;
            
            // 应用初始FOV设置
            if (_settingsController == null && movementConfig != null && playerCamera != null)
            {
                playerCamera.fieldOfView = movementConfig.Fov;
            }

            // 锁定光标
            PlayerController.LockAndHideCursor();


            IsInitialized = true;
        }




        private void RefreshEventSubscription()
        {
            UnsubscribeToEvents();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            PlayerInputEvents.OnLookInput += HandleLookInput;
            if (_settingsController != null)
            {
                _settingsController.FieldOfViewChanged += OnFieldOfViewChanged;
                _settingsController.MouseSensitivityChanged += OnMouseSensitivityChanged;
            }
        }

        private void OnMouseSensitivityChanged(float obj)
        {
            if (movementConfig != null)
            {
                movementConfig.SetMouseSensitivity = obj;
            }
        }

        private void OnFieldOfViewChanged(float obj)
        {
            if (movementConfig != null)
            {
                movementConfig.SetFOV = obj;
            }
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = obj;
            }
        }

        public void Update()
        {
            if (!IsEnabled)
            {
                return;
            }
            
            HandleMouseLook();
        }

        private void LateUpdate()
        {
            if (!IsEnabled||!IsInitialized)
            {
                return;
            }
            HandleCameraTilt();
        }

        public void FixedUpdate() { }
        
        private void HandleLookInput(Vector2 input)
        {
            // 直接使用输入，无需存储目标值
            _currentLookInput = input;
//            LogUtil.Log($"Look Input: {input}", false);
        }
        
        private void HandleMouseLook()
        {
            // 如果光标未锁定,不处理摄像机输入
            if (!_playerController.IsCursorLocked)
            {
                return;
            }
            
            Vector2 lookInput;
            
            if (useRawInput)
            {
                // 使用原始输入，无延迟
                lookInput = _currentLookInput;
            }
            else
            {
                // 使用平滑输入（传统方式）
                lookInput = Vector2.Lerp(_currentLookInput, _currentLookInput, Smoothness * Time.deltaTime);
            }
            
            if (lookInput.magnitude < 0.01f) return;
            
            // 计算旋转增量
            float mouseX, mouseY;
            
            if (frameRateIndependent)
            {
                // 帧率无关的控制（推荐用于FPS）
                mouseX = lookInput.x * MouseSensitivity * Time.deltaTime;
                mouseY = lookInput.y * MouseSensitivity * Time.deltaTime;
            }
            else
            {
                // 固定增量（可能在高帧率下过于敏感）
                mouseX = lookInput.x * MouseSensitivity * 0.01f; // 0.01f作为基础倍率
                mouseY = lookInput.y * MouseSensitivity * 0.01f;
            }
            
            // 水平旋转（Y轴）- 旋转角色身体
            _yRotation += mouseX;
            playerBody.rotation = Quaternion.Euler(0f, _yRotation, 0f);
            
            // 垂直旋转（X轴）- 旋转摄像机
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, minVerticalAngle, maxVerticalAngle);
            
            // 应用摄像机旋转（包含倾斜效果）
            playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, _currentTilt);
            
            // 对于原始输入模式，重置输入以避免连续旋转
            if (useRawInput)
            {
                _currentLookInput = Vector2.zero;
            }
        }

        /// <summary>
        ///  处理摄像机倾斜效果
        /// </summary>
        public void HandleCameraTilt()
        {
            float targetRollAngle = 0f;
            if (_runtimeData.IsWallRunning)
            {
                Vector3 wallNormal = _runtimeData.WallNormal;
                Vector3 playerLookDirection = transform.forward;
                
                float dot = Mathf.Abs(Vector3.Dot(playerLookDirection, wallNormal));
                
                float rollMultiplier = 1 - dot;

                // 根据墙壁在左边还是右边决定基础翻滚方向和角度
                float baseRollAngle = -_runtimeData.GetWallSide(transform) * movementConfig.MaxCameraTiltAngle;

                // 应用系数
                targetRollAngle = baseRollAngle * rollMultiplier;
            }

            _currentTilt = Mathf.Lerp(_currentTilt, targetRollAngle, movementConfig.CameraTiltSpeed * Time.deltaTime);

            // 应用旋转
             playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, _currentTilt);
        }
        /// <summary>
        /// 平滑地将摄像机朝向指定方向（类似手柄辅助瞄准，不阻止玩家输入）
        /// </summary>
        /// <param name="targetDirection">目标方向（世界坐标）</param>
        /// <param name="assistStrength">辅助强度（可选，默认使用Inspector设置）</param>
        /// <param name="duration">辅助持续时间（可选，默认使用Inspector设置）</param>
        public void SmoothLookAtDirection(Vector3 targetDirection, float? assistStrength = null, float? duration = null)
        {
            if (targetDirection.sqrMagnitude < 0.01f)
            {
                Debug.LogWarning("目标方向不能为零向量");
                return;
            }
            var defaultAssistStrength = movementConfig.DefaultAssistStrength;
            var defaultAssistDuration = movementConfig.DefaultAssistDuration;
            
            // 使用传入参数或默认值
            float actualAssistStrength = assistStrength ?? defaultAssistStrength;
            float actualDuration = duration ?? defaultAssistDuration;

            // 停止之前的辅助瞄准
            StopLookAssist();

            Vector3 normalizedDirection = targetDirection.normalized;

            // 计算目标旋转角度
            float targetY = Mathf.Atan2(normalizedDirection.x, normalizedDirection.z) * Mathf.Rad2Deg;
            float horizontalDistance = Mathf.Sqrt(normalizedDirection.x * normalizedDirection.x +
                                                  normalizedDirection.z * normalizedDirection.z);
            float targetX = -Mathf.Atan2(normalizedDirection.y, horizontalDistance) * Mathf.Rad2Deg;
            targetX = Mathf.Clamp(targetX, -90f, 90f);

            // 启动辅助瞄准协程
            StartCoroutine(LookAssistCoroutine(targetX, targetY, actualAssistStrength, actualDuration));
        }
        /// <summary>
        /// 停止视角辅助
        /// </summary>
        public void StopLookAssist()
        {
            if (_lookAssistCoroutine != null)
            {
                StopCoroutine(_lookAssistCoroutine);
                _lookAssistCoroutine = null;
            }
        }
        private System.Collections.IEnumerator LookAssistCoroutine(float targetX, float targetY, float assistStrength,
            float duration)
        {
            float timer = 0f;

            while (duration < 0f || timer < duration)
            {
                // 计算当前角度与目标角度的差值
                float deltaX = Mathf.DeltaAngle(_targetXRotation, targetX);
                float deltaY = Mathf.DeltaAngle(_targetYRotation, targetY);

                // 应用辅助力，不覆盖玩家输入，而是添加到目标值上
                float assistForceX = deltaX * assistStrength * Time.deltaTime * movementConfig.AssistResponseSpeed;
                float assistForceY = deltaY * assistStrength * Time.deltaTime * movementConfig.AssistResponseSpeed;

                _targetXRotation += assistForceX;
                _targetYRotation += assistForceY;

                // 限制垂直角度
                _targetXRotation = Mathf.Clamp(_targetXRotation, -90f, 90f);

                timer += Time.deltaTime;
                yield return null;
            }
        }
        
        /// <summary>
        /// 设置摄像机震动效果
        /// </summary>
        public void AddCameraShake(float intensity, float duration)
        {
            // 可以在这里实现摄像机震动效果
            StartCoroutine(CameraShakeCoroutine(intensity, duration));
        }
        
        private System.Collections.IEnumerator CameraShakeCoroutine(float intensity, float duration)
        {
            Vector3 originalPosition = playerCamera.transform.localPosition;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                
                playerCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            playerCamera.transform.localPosition = originalPosition;
        }
        
        /// <summary>
        /// 重置摄像机旋转
        /// </summary>
        public void ResetRotation()
        {
            _xRotation = 0f;
            _yRotation = 0f;
            _currentTilt = 0f;
            _targetTilt = 0f;
            
            playerBody.rotation = Quaternion.identity;
            playerCamera.transform.localRotation = Quaternion.identity;
        }
        
        /// <summary>
        /// 设置摄像机灵敏度（通过配置文件）
        /// </summary>
        public void SetSensitivity(float sensitivity)
        {
            // 由于我们使用ScriptableObject，这里只能通过运行时修改来调整
            // 在实际项目中，可以考虑添加运行时灵敏度倍率
            Debug.Log($"Note: Sensitivity is controlled by PlayerMovementConfig. Current: {MouseSensitivity}");
        }
        
        /// <summary>
        /// 切换输入模式（原始输入vs平滑输入）
        /// </summary>
        public void SetRawInputMode(bool enabled)
        {
            useRawInput = enabled;
            Debug.Log($"Raw input mode: {(enabled ? "Enabled" : "Disabled")}");
        }
        
        /// <summary>
        /// 设置是否使用帧率无关控制
        /// </summary>
        public void SetFrameRateIndependent(bool enabled)
        {
            frameRateIndependent = enabled;
            Debug.Log($"Frame rate independent control: {(enabled ? "Enabled" : "Disabled")}");
        }
        
        
        public void UnsubscribeToEvents()
        {
            PlayerInputEvents.OnLookInput -= HandleLookInput;
            if (_settingsController != null)
            {
                _settingsController.FieldOfViewChanged -= OnFieldOfViewChanged;
                _settingsController.MouseSensitivityChanged -= OnMouseSensitivityChanged;
            }
        }
        public void CleanUp()
        {
          
        }
        private void OnDisable()
        {
            UnsubscribeToEvents();
        }
    }
}
