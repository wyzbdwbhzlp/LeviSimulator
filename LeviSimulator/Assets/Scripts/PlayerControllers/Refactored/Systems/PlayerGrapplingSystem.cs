using System;
using DG.Tweening;
using Game.Audio;
using PlayerControllers.Grapple;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Object = UnityEngine.Object;

namespace PlayerControllers.Refactored.Systems
{
    public class PlayerGrapplingSystem: MonoBehaviour, IPlayerSystem
    {
        private GrappleCableRenderer _grappleCableRenderer;
        private GrappleHook _grappleHook;
        private PlayerGrapplingConfig _grappleConfig;
        private Transform _grappleMuzzleTransform;
        public void SetGrappleSetting(PlayerGrapplingConfig grappleConfig, Transform grappleMuzzleTransform)
        {
            _grappleConfig = grappleConfig;
            _grappleHook?.SetConfig(grappleConfig);
            _grappleMuzzleTransform= grappleMuzzleTransform;
        }

        
        public void Initialize(PlayerController playerController, ScriptableObject playerConfig)
        {
            _grappleHook = new GrappleHook();
            _grappleHook.Initialize(playerController);
            
            _grappleCableRenderer = _grappleMuzzleTransform.gameObject.GetComponent<GrappleCableRenderer>();
            _grappleCableRenderer.SetGrappleHook(_grappleHook);

            RefreshEventSubscription();

            IsInitialized = true;

        }

        private void RefreshEventSubscription()
        {
            UnsubscribeToEvents();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            PlayerInputEvents.OnGrapplePressed += StartGrapple;
            PlayerInputEvents.OnGrappleReleased += StopGrapple;
        }
        
        public void Update()
        {
            if (!IsEnabled)
            {
                return;
            }

            if (_grappleHook != null && _grappleHook.GrappleState != GrappleState.Idle)
            {
                _grappleHook.UpdateGrapple();
            }
            _grappleCableRenderer?.UpdateCable();
        }

        public void FixedUpdate()
        {
            if (!IsEnabled)
            {
                return;
            }

            if (_grappleHook != null && _grappleHook.GrappleState == GrappleState.Grappling)
            {
                _grappleHook.ApplyGrappleForce();
            }
        }

        public void UnsubscribeToEvents()
        {
            
            PlayerInputEvents.OnGrapplePressed -= StartGrapple;
            PlayerInputEvents.OnGrappleReleased -= StopGrapple;
        }

        private void OnDisable()
        {
            CleanUp();
            UnsubscribeToEvents();
        }

        public void CleanUp()
        {
            _grappleHook?.StopGrapple();
            _inputDirectionTween?.Kill();
        }
        


        public bool IsEnabled { get; set; }
        public bool IsInitialized { get; set; } = false;

        // 公开方法供外部调用
        public void StartGrapple()
        {
            AudioEventHandler.CallPlayOneShotFor2D(AudioNames.钩索发射声音A);
            _grappleHook?.StartGrapple();
        }

        public void StopGrapple() => _grappleHook?.StopGrapple();
        public bool IsGrappling => _grappleHook?.GrappleState == GrappleState.Grappling;
        
        private Tweener _inputDirectionTween;
    }

    public class GrappleHook : IGrapple
    {
        [LabelText("钩爪预制体")]private GameObject grapplePrefab; 
        [ReadOnly][LabelText("钩爪实例")]private GameObject grapplePrefabInstance; 
        [LabelText("钩爪发射器状态")]private GrappleState grappleState;
        [Title("钩爪状态")]
        [ReadOnly] private Vector3 _grapplePoint; // 钩爪抓取点
        [ReadOnly] private float _grappleFlightTime; // 钩爪飞行时间
        [ReadOnly] private bool _isInvalidGrapple = false; // 是否抓取到了无效对象
        private AudioSourceWrapper _GrapplingaudioSourceWrapper;
        
        [ShowInInspector][ReadOnly]private Transform _cameraTransform; // 摄像机位置引用
        [ShowInInspector][ReadOnly]private PlayerController _playerController; 
        private Transform _grappleTipTransform; // 钩爪尖端位置引用(钩爪起始点)
        private PlayerGrapplingConfig _grappleConfig;
        private Tweener _inputDirectionTween;
        private Vector3 _smoothedWorldInputDirection;

        // IGrapple 接口实现
        public GrappleState GrappleState 
        { 
            get => grappleState; 
            set => grappleState = value; 
        }
        
        public Vector3 GrapplePoint 
        { 
            get => _grapplePoint; 
            set => _grapplePoint = value; 
        }
        
        public Transform GrappleTipTransform => _grappleTipTransform;
        
        public bool IsInvalidGrapple 
        { 
            get => _isInvalidGrapple; 
            set => _isInvalidGrapple = value; 
        }

        public void Initialize(PlayerController playerController)
        {
            grappleState = GrappleState.Idle; 
            _playerController = playerController;
            _grappleConfig = playerController.GrapplingConfig;
            _cameraTransform = playerController.CameraSystem.transform;
            _grappleTipTransform = playerController.GrapplingMuzzle; // 临时设置，应该设置为实际的钩爪发射点

     
        }

        public void SetConfig(PlayerGrapplingConfig config)
        {
            _grappleConfig = config;
        }

        public void StartGrapple()
        {
            if (grappleState != GrappleState.Idle)
            {
                LogUtil.LogWarning("已经在钩爪状态中，无法再次开始钩爪");
                return;
            }
            
            RaycastHit hit;
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out hit, _grappleConfig.MaxGrappleDistance, _grappleConfig.GrappleLayer))
            {
                grappleState = GrappleState.Flight;
                _grapplePoint = hit.point;
                
                var grappleFlightDistance = Vector3.Distance(_playerController.transform.position, _grapplePoint);
                _grappleFlightTime = grappleFlightDistance / _grappleConfig.GrappleSpeed;
                
                LogUtil.Log($"钩爪命中目标: {hit.collider.name}, 距离: {grappleFlightDistance}");
            }
            else
            {
                _grapplePoint = _cameraTransform.position + _cameraTransform.forward * _grappleConfig.MaxGrappleDistance;
                grappleState = GrappleState.Flight;
                _isInvalidGrapple = true;
                _grappleFlightTime = _grappleConfig.MaxGrappleDistance / _grappleConfig.GrappleSpeed;
                
                LogUtil.LogWarning("未命中任何可钩爪的目标，钩爪点设置为最大距离");
            }
        }

        public void StopGrapple()
        {
            if (grapplePrefabInstance)
            {
                Object.Destroy(grapplePrefabInstance);
                grapplePrefabInstance = null;
            }
            
            _inputDirectionTween?.Kill();
            _inputDirectionTween = null;
            _GrapplingaudioSourceWrapper?.Stop();
            
            EventBroadcaster.CallPlayerEndGrappleEvent();
            _isInvalidGrapple = false;
            grappleState = GrappleState.Idle;
            
            LogUtil.Log("钩爪停止");
        }

        public void UpdateGrapple()
        {
            if (_grappleFlightTime > 0)
            {
                _grappleFlightTime -= Time.deltaTime;
                return;
            }
            
            if (!grapplePrefabInstance && !_isInvalidGrapple)
            {
                // 实例化钩爪预制体（如果需要的话）
                if (grapplePrefab != null)
                {
                    grapplePrefabInstance = Object.Instantiate(grapplePrefab, _grapplePoint, Quaternion.identity);
                    LogUtil.Log($"钩爪实例化成功，位置: {_grapplePoint}");
                }
                AudioEventHandler.CallPlayOneShotFor2D(AudioNames.钩索命中声);
                grappleState = GrappleState.Grappling;
                _GrapplingaudioSourceWrapper=AudioEventHandler.CallPlayOneShotFor2D(AudioNames.钩索拉动声);//todo 需要loop
                
                // 切换到钩爪状态 - 这里需要根据您的状态机系统进行调整
                //todo  _playerController.ChangeParentStatus<GrapplingStatusStrategy>();
                
                LogUtil.Log("开始钩爪摆动");
            }
            
            if (_isInvalidGrapple)
            {
                StopGrapple();
            }
        }

        public void ApplyGrappleForce()
        {
            if (grappleState != GrappleState.Grappling || _playerController == null)
                return;

            Vector3 playerPosition = _playerController.transform.position;
            Vector3 grappleDirection = (_grapplePoint - playerPosition).normalized;
            float grappleDistance = Vector3.Distance(playerPosition, _grapplePoint);

            // 检查销毁条件
            if (CheckDestroyCondition(_grapplePoint - playerPosition))
            {
                StopGrapple();
                return;
            }
            

            Vector3 grappleForce = CalculateGrappleForce(grappleDirection, grappleDistance);
            
            // 应用力到玩家刚体
            var playerMovementSystem = _playerController.MovementSystem;
            if (!IsPlayerSpeeding())
            {
                var forceMagnitude = grappleForce.magnitude;
                bool isOverMaxForce = forceMagnitude > _grappleConfig.MaxGrappleForce;
                if (!isOverMaxForce)
                {
                    playerMovementSystem.AddPlayerRigidbodyForce(grappleForce, ForceMode.Acceleration);
                }
                else
                {
                    Vector3 limitedForce = grappleForce.normalized * _grappleConfig.MaxGrappleForce;
                    playerMovementSystem.AddPlayerRigidbodyForce(limitedForce, ForceMode.Acceleration);
                }
            }
        }

        /// <summary>
        /// F = -k * x - d * v
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private Vector3 CalculateGrappleForce(Vector3 direction, float distance)
        {
            if (_grappleConfig == null) return Vector3.zero;

            if (_grappleConfig.UsePlayerViewParameters)
            {
                Vector3 viewDirection = _cameraTransform.forward;
                direction = Vector3.Lerp(direction, viewDirection.normalized, _grappleConfig.PlayerViewParameters).normalized;
            }

            if (_grappleConfig.UsePlayerInputDirection)
            {
                var moveInput = _playerController.RuntimeData.MoveInput;
                Vector3 localInputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
                Vector3 targetWorldInputDirection = Vector3.zero;

                if (localInputDirection.sqrMagnitude > 0.01f)
                {
                    Vector3 cameraForward = Vector3.Scale(_cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
                    Vector3 cameraRight = Vector3.Scale(_cameraTransform.right, new Vector3(1, 0, 1)).normalized;
                    targetWorldInputDirection = (cameraRight * localInputDirection.x + cameraForward * localInputDirection.z).normalized;
                }
                
                if (_inputDirectionTween == null || !_inputDirectionTween.IsActive())
                {
                    _inputDirectionTween = DOTween.To(() => _smoothedWorldInputDirection,
                            x => _smoothedWorldInputDirection = x,
                            targetWorldInputDirection,
                            0.25f) 
                        .SetEase(Ease.OutCubic); 
                }
                else
                {
                    _inputDirectionTween.ChangeEndValue(targetWorldInputDirection, true);
                }

                if (_smoothedWorldInputDirection.sqrMagnitude > 0.01f)
                {
                    Vector3 playerInputForceBuff = _smoothedWorldInputDirection * _grappleConfig.PlayerInputDirectionBuff;
                    direction = Vector3.Lerp(direction, playerInputForceBuff, _grappleConfig.PlayerInputDirectionParameters).normalized;
                }
            }

            Vector3 springForce = direction * _grappleConfig.SpringStrength * distance;
            Vector3 dampingForce = -_playerController.RuntimeData.MoveDirection * _grappleConfig.Damping;
            return springForce + dampingForce;
        }

        private bool IsPlayerSpeeding()
        {
            var playerSpeed = _playerController.MovementSystem.Rigidbody.linearVelocity.magnitude;
            return playerSpeed > _grappleConfig.MaxPlayerVelocityOnGrapple;
        }

        private bool CheckDestroyCondition(Vector3 delta)
        {
            if (delta.magnitude <= _grappleConfig.StableZone)
            {
                LogUtil.Log("钩爪已进入稳定区，准备销毁");
                return true;
            }

            if (_grappleConfig.UsePlayerViewAngleCheck)
            {
                Vector3 playerForward = _playerController.CameraSystem.transform.forward;
                float angle = Vector3.Angle(playerForward.normalized, delta.normalized);
                if (angle > _grappleConfig.MaxAllowedViewAngle)
                {
                    LogUtil.Log($"钩爪与玩家视角夹角过大({angle}°)，准备销毁");
                    return true;
                }
            }


            return false;
        }
    }
}