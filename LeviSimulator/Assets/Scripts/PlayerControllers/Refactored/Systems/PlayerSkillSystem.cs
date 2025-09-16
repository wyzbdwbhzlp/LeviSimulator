using System;
using System.Collections;
using System.Collections.Generic;
using GlobalGameManager;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace PlayerControllers.Refactored.Systems
{
    public class PlayerSkillSystem : MonoBehaviour, IPlayerSystem
    {
        [Title("运行中数据")] [ReadOnly] [SerializeField] [LabelText("冲刺冷却计时")]
        private float dashCoolDownTimer;

        [ReadOnly] [SerializeField] [LabelText("当前冲刺数量")]
        private int currentDashCount;

        [ReadOnly] [SerializeField] [LabelText("子弹时间计时")]
        private float bulletTimeEnergy;


        private Vector3 _dashDirectionSmooth = Vector3.forward;
        private Vector3 _lastNonZeroDashDirection = Vector3.forward;


        private Coroutine _rechargeCoroutine;

        private PlayerController _playerController;
        private PlayerSkillConfig _skillConfig;
        private PlayerRuntimeData _playerRuntimeData;

        public void Initialize(PlayerController playerController, ScriptableObject playerConfig)
        {
            _skillConfig = playerConfig as PlayerSkillConfig;
            if (_skillConfig == null)
            {
                Debug.LogError("PlayerSkillConfig未正确分配");
                return;
            }

            _playerController = playerController;
            _playerRuntimeData = playerController.RuntimeData;

            currentDashCount = _skillConfig.MaxDashCount;
            bulletTimeEnergy = _skillConfig.MaxBulletTimeEnergy;

            // // 初始化冲刺方向为摄像机前方（水平）
            // UpdateDefaultDashDirection(Vector2.zero);
            _lastNonZeroDashDirection = _dashDirectionSmooth;
        }

        private void OnEnable()
        {
            PlayerInputEvents.OnBulletTimePressed += HandleBulletTimePressed;
            PlayerInputEvents.OnDashPressed += TryUseDash;
            // PlayerInputEvents.OnLookInput += UpdateDefaultDashDirection;
        }


        private void OnDisable()
        {
            PlayerInputEvents.OnBulletTimePressed -= HandleBulletTimePressed;
            PlayerInputEvents.OnDashPressed -= TryUseDash;
            // PlayerInputEvents.OnLookInput -= UpdateDefaultDashDirection;
        }

        private void HandleBulletTimePressed()
        {
            if (_playerRuntimeData.IsInBulletTime)
            {
                StopBulletTime();
            }
            else
            {
                TryUseBulletTime();
            }
        }


        public void Update()
        {
            UpdateBulletTimeEnergy();
        }


        public void FixedUpdate()
        {
        }

        public void Cleanup()
        {
        }

        #region 冲刺相关

        private void TryUseDash()
        {
            if (currentDashCount > 0)
            {
                currentDashCount--;
                if (_rechargeCoroutine == null)
                {
                    _rechargeCoroutine = StartCoroutine(DashCoolDownCoroutine());
                }

                _playerController.MovementSystem.ApplyDash(CalculateDashVelocity());
                LogUtil.Log($"使用冲刺成功，目前剩余次数:{currentDashCount}");
            }
            else
            {
                LogUtil.Log("目前技能尚未冷却完毕");
            }
        }

        private Vector3 CalculateDashVelocity()
        {
            //Vector2 moveInput = _playerRuntimeData.MoveInput;
            Transform cameraTransform = _playerController.CameraSystem.Camera.transform;
            // 获取摄像机方向向量
            Vector3 cameraForward = cameraTransform.forward;

            // 计算混合后的方向向量
            Vector3 blendedForward = GetBlendedDirection(cameraForward);

            // 计算目标冲刺方向
            Vector3 targetDirection = CalculateTargetDirection(blendedForward);

            // // 应用方向平滑
            // Vector3 smoothedDirection = ApplyDirectionSmoothing(targetDirection);

            // 应用垂直角度限制
            //Vector3 finalDirection = ApplyVerticalAngleLimitation(smoothedDirection);
            Vector3 finalDirection = ApplyVerticalAngleLimitation(targetDirection);

            // 计算并返回冲刺速度
            return finalDirection * (_skillConfig.DashDistance / _skillConfig.DashDuration);
        }

        /// <summary>
        /// 根据垂直保留系数混合水平和完整方向向量
        /// </summary>
        private Vector3 GetBlendedDirection(Vector3 cameraDirection)
        {
            // 水平分量（y=0）
            Vector3 horizontalDirection = new Vector3(cameraDirection.x, 0f, cameraDirection.z);

            // 处理极小向量的情况
            if (horizontalDirection.sqrMagnitude < 0.0001f)
            {
                horizontalDirection = cameraDirection.z > 0 ? Vector3.forward :
                    cameraDirection.x > 0 ? Vector3.right : Vector3.forward;
            }
            else
            {
                horizontalDirection.Normalize();
            }

            // 完整方向向量（归一化）
            Vector3 fullDirection = cameraDirection.normalized;

            // 根据 verticalRetention 在水平和完整方向间混合
            return Vector3.Lerp(horizontalDirection, fullDirection,
                Mathf.Clamp01(_skillConfig.VerticalRetention));
        }

        /// <summary>
        /// 根据输入计算目标冲刺方向
        /// </summary>
        private Vector3 CalculateTargetDirection(Vector3 blendedForward)
        {
            Vector3 targetDirection = blendedForward;
            // 无输入时优先使用摄像机前方方向

            // 如果玩家当前有移动速度，可以考虑使用速度方向作为参考
            Vector3 currentVelocity = _playerRuntimeData.HorizontalVelocity;
            if (currentVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 velocityDirection = currentVelocity.normalized;
                // 在摄像机前方和当前速度方向之间进行混合
                targetDirection = Vector3.Slerp(blendedForward, velocityDirection, 0.3f).normalized;
            }


            // 保底方向处理
            return targetDirection.sqrMagnitude < 0.0001f ? Vector3.forward : targetDirection;
        }

        // /// <summary>
        // /// 应用指数平滑到方向向量
        // /// </summary>
        // private Vector3 ApplyDirectionSmoothing(Vector3 targetDirection)
        // {
        //     // 帧率无关的指数平滑: alpha = 1 - exp(-lambda * dt)
        //     float smoothingAlpha = 1f - Mathf.Exp(-_skillConfig.DashDirectionSmoothing * Time.deltaTime);
        //     _dashDirectionSmooth = Vector3.Slerp(_dashDirectionSmooth, targetDirection, smoothingAlpha);
        //
        //     return _dashDirectionSmooth.normalized;
        // }

        /// <summary>
        /// 应用垂直角度限制（可选）
        /// </summary>
        private Vector3 ApplyVerticalAngleLimitation(Vector3 direction)
        {
            if (!_skillConfig.UseMaxVerticalAngle)
                return direction;

            float maxVerticalComponent = Mathf.Sin(_skillConfig.MaxVerticalAngleDeg * Mathf.Deg2Rad);

            if (Mathf.Abs(direction.y) > maxVerticalComponent)
            {
                direction.y = Mathf.Sign(direction.y) * maxVerticalComponent;
                direction = direction.normalized;
            }

            return direction;
        }

        // /// <summary>
        // /// 更新默认冲刺方向（在初始化和摄像机旋转时调用）
        // /// </summary>
        // private void UpdateDefaultDashDirection(Vector2 obj)
        // {
        //     if (_playerController?.CameraSystem?.Camera?.transform != null)
        //     {
        //         Vector3 cameraForward = _playerController.CameraSystem.Camera.transform.forward;
        //         Vector3 horizontalForward = new Vector3(cameraForward.x, 0f, cameraForward.z);
        //
        //         if (horizontalForward.sqrMagnitude > 0.0001f)
        //         {
        //             _dashDirectionSmooth = horizontalForward.normalized;
        //         }
        //         else
        //         {
        //             _dashDirectionSmooth = Vector3.forward;
        //         }
        //     }
        //     else
        //     {
        //         _dashDirectionSmooth = Vector3.forward;
        //     }
        // }

        private IEnumerator DashCoolDownCoroutine()
        {
            while (currentDashCount < _skillConfig.MaxDashCount)
            {
                dashCoolDownTimer = _skillConfig.DashCooldown;
                while (dashCoolDownTimer > 0)
                {
                    dashCoolDownTimer -= Time.deltaTime;
                    yield return null;
                }

                currentDashCount++;
                LogUtil.Log($"冲刺冷却完毕{currentDashCount}");
            }

            _rechargeCoroutine = null;
        }

        #endregion

        private void TryUseBulletTime()
        {
            if (bulletTimeEnergy >= _skillConfig.MinEnableBulletTimeEnergy)
            {
                _playerRuntimeData.IsInBulletTime = true;
                GlobalManager.Instance.timeManager.StartBulletTime(_skillConfig.BulletTimeScale);
            }
            else
            {
                LogUtil.Log("子弹时间能量不足");
            }
        }

        private void StopBulletTime()
        {
            _playerRuntimeData.IsInBulletTime = false;
            GlobalManager.Instance.timeManager.EndBulletTime();
        }

        /// <summary>
        /// 更新子弹时间能量
        /// </summary>
        private void UpdateBulletTimeEnergy()
        {
            if (_playerRuntimeData.IsInBulletTime)
            {
                bulletTimeEnergy -= Time.unscaledDeltaTime;
                if (bulletTimeEnergy <= 0)
                {
                    bulletTimeEnergy = 0;
                    StopBulletTime();
                }
            }
            else if (!_playerRuntimeData.IsInBulletTime &&
                     bulletTimeEnergy < _skillConfig.MaxBulletTimeEnergy)
            {
                bulletTimeEnergy += Time.unscaledDeltaTime * _skillConfig.BulletTimeCooldownFactor;
                if (bulletTimeEnergy > _skillConfig.MaxBulletTimeEnergy)
                {
                    bulletTimeEnergy = _skillConfig.MaxBulletTimeEnergy;
                }
            }
        }

        public bool IsEnabled { get; set; }
    }
}