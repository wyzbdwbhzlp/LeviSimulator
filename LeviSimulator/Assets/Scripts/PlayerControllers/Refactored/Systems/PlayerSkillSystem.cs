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

        [ReadOnly] [ShowInInspector] [LabelText("当前冲刺数量")]
        private int currentDashCount=>_playerRuntimeData.DashCount;

        [ReadOnly] [ShowInInspector] [LabelText("子弹时间计时")]
        private float bulletTimeEnergy=>_playerRuntimeData.BulletTimeEnergy;


        private Vector3 _dashDirectionSmooth = Vector3.forward;


        private Coroutine _rechargeCoroutine;

        private PlayerController _playerController;
        private PlayerSkillConfig _skillConfig;
        private PlayerRuntimeData _playerRuntimeData;
        public bool IsInitialized { get; set; } = false;
        private TimeManager timeManager=>GlobalManager.Instance?.timeManager;

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

            _playerRuntimeData.DashCount = _skillConfig.MaxDashCount;
            _playerRuntimeData.BulletTimeEnergy = _skillConfig.MaxBulletTimeEnergy;
            RefreshEventSubscription();
            IsInitialized = true;
        }

        private void RefreshEventSubscription()
        {
            UnsubscribeToEvents();
            SubscribeToEvents();
            // PlayerInputEvents.OnLookInput += UpdateDefaultDashDirection;
        }


        private void OnDisable()
        {
            UnsubscribeToEvents();
            // PlayerInputEvents.OnLookInput -= UpdateDefaultDashDirection;
        }

        public void UnsubscribeToEvents()
        {
            PlayerInputEvents.OnBulletTimePressed -= HandleBulletTimePressed;
            PlayerInputEvents.OnDashPressed -= TryUseDash;
        }

        private void SubscribeToEvents()
        {
            PlayerInputEvents.OnBulletTimePressed += HandleBulletTimePressed;
            PlayerInputEvents.OnDashPressed += TryUseDash;
        }
        public void CleanUp()
        {
           
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
        

        #region 冲刺相关

        private void TryUseDash()
        {
            if (currentDashCount > 0)
            {
                _playerRuntimeData.ConsumeDash();// 消耗一次冲刺
                if (_rechargeCoroutine == null)
                {
                    _rechargeCoroutine = StartCoroutine(DashCoolDownCoroutine());
                }

                _playerController.MovementSystem.ApplyDash(CalculateDashVelocity());
                _playerController.RuntimeData.IsDashing= true;
                StartCoroutine(DashTimerCoroutine(_skillConfig.DashDuration));
                LogUtil.Log($"使用冲刺成功，目前剩余次数:{currentDashCount}");
            }
            else
            {
                LogUtil.Log("目前技能尚未冷却完毕");
            }
        }
        private IEnumerator DashTimerCoroutine(float dashDuration)
        {
            yield return new WaitForSeconds(dashDuration);
            _playerController.RuntimeData.IsDashing= false;
            // 冲刺结束后的逻辑（如果有）
        }
        
        private Vector3 CalculateDashVelocity()
        {
            // 始终使用摄像机朝向作为冲刺方向
            Vector3 targetDirection = _playerController.CameraSystem.Camera.transform.forward;
            
            // 禁止向下冲刺
            if (targetDirection.y < 0)
            {
                targetDirection.y = 0;
                targetDirection = targetDirection.normalized;
            }
            else
            {
                targetDirection = targetDirection.normalized;
            }
            
            // 应用垂直角度限制（如果配置中启用）
            targetDirection = ApplyVerticalAngleLimitation(targetDirection);
            LogUtil.Log($"冲刺方向:{targetDirection}");
            return targetDirection * (_skillConfig.DashDistance / _skillConfig.DashDuration);
        }
 

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

                _playerRuntimeData.RecoverDash();
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
                timeManager.StartBulletTime(_skillConfig.BulletTimeScale);
            }
            else
            {
                LogUtil.Log("子弹时间能量不足");
            }
        }

        private void StopBulletTime()
        {
            _playerRuntimeData.IsInBulletTime = false;
            timeManager.EndBulletTime();
        }

        /// <summary>
        /// 更新子弹时间能量
        /// </summary>
        private void UpdateBulletTimeEnergy()
        {
            if (_playerRuntimeData.IsInBulletTime)
            {
                _playerRuntimeData.BulletTimeEnergy -= Time.unscaledDeltaTime;
                if (bulletTimeEnergy <= 0)
                {
                    _playerRuntimeData.BulletTimeEnergy = 0;
                    StopBulletTime();
                }
            }
            else if (!_playerRuntimeData.IsInBulletTime &&
                     bulletTimeEnergy < _skillConfig.MaxBulletTimeEnergy)
            {
                _playerRuntimeData.BulletTimeEnergy += Time.unscaledDeltaTime * _skillConfig.BulletTimeCooldownFactor;
                if (bulletTimeEnergy > _skillConfig.MaxBulletTimeEnergy)
                {
                    _playerRuntimeData.BulletTimeEnergy = _skillConfig.MaxBulletTimeEnergy;
                }
            }
        }

        public bool IsEnabled { get; set; }
    }
}