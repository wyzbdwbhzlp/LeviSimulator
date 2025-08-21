// Assets/Scripts/PlayerControllers/PlayerCharacterStatusStrategy/HierarchicalBaseState.cs

using System.Collections;
using System.Collections.Generic;
using PlayerControllers.PlayerCharacterStatusStrategy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategyHFSM
{
    public abstract class HierarchicalBaseState:IPlayerCharacterStatusStrategy
    {
        protected PlayerInputRouter _playerInputRouter;
        private HierarchicalBaseState _currentSubState;
        protected PlayerInputRouter Ctx;
        public HierarchicalBaseState CurrentSubState => _currentSubState;

        public void OnEnter(PlayerInputRouter input)
        {
            _playerInputRouter = input;
            this.Ctx = input;
            EnterState();
        }

        public void OnExit()
        {
            if (_currentSubState != null)
            {
                _currentSubState.ExitState();
                _currentSubState = null;
            }
            ExitState();
        }

        public void LogicUpdate()
        {
            _currentSubState?.UpdateStates();
            UpdateStates();
        }
        
        public void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            // 优先让子状态处理输入
            if (_currentSubState != null && _currentSubState.HandleSubStateInput(obj))
            {
                return;
            }
            // 如果子状态不处理，则由当前状态处理
            HandleInput(obj);
        }

        // 子类重写以实现自己的进入逻辑
        protected abstract void EnterState();
        // 子类重写以实现自己的退出逻辑
        protected abstract void ExitState();
        // 子类重写以实现自己的更新逻辑（状态切换检查）
        protected abstract void UpdateStates();
        // 子类重写以处理输入
        protected abstract void HandleInput(InputAction.CallbackContext obj);
        // 子状态重写以决定是否处理输入
        protected virtual bool HandleSubStateInput(InputAction.CallbackContext obj)
        {
            return false; // 默认不处理，交由父状态
        }

        protected void SwitchSubState<T>()where T :HierarchicalBaseState
        {
            var newState = PlayerCharacterStatusStrategyFactory.GetState<T>();
            _currentSubState?.OnExit();
            _currentSubState = newState;
            _currentSubState?.OnEnter(this.Ctx);
            DebugSpeedShowController.Instance?.SetParentState(newState);
        }
        
        public void StartCrouchOrSliding() 
        {
            if (Ctx == null)
            {
                LogUtil.LogError("PlayerInputRouter 未初始化，无法执行 StartCrouchOrSliding");
                return;
            }
            var rd = Ctx.MovementController.PlayerRigidbody;
            var groundNormal = Ctx.MovementController.GroundNormal;
            var currentPlayerRdVelocity = Ctx.MovementController.CurrentPlayerRdVelocity;
            var currentPlayerRdHorizontalVelocityMagnitude = Ctx.MovementController.CurrentPlayerRdHorizontalVelocityMagnitude;
            var minSlideSpeed = Ctx.MovementController.MinSlideSpeed;
            var slopeSlideMinAngle = Ctx.MovementController.SlopeSlideMinAngle;
            var fallSpeedToSlideBoostMultiplier = Ctx.MovementController.FallSpeedToSlideBoostMultiplier;
            

            var slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
                    
            if (currentPlayerRdHorizontalVelocityMagnitude > minSlideSpeed || slopeAngle >= slopeSlideMinAngle)
            {
                LogUtil.Log("速度足够，进入滑铲状态");
                SwitchSubState<SlidingStatusStrategy>();
                var verticalSpeedBonus = Mathf.Max(0, -currentPlayerRdVelocity.y);
                // 如果有来自下落的速度增益，则应用它
                if (verticalSpeedBonus > 0)
                {
                    Vector3 forwardDir = currentPlayerRdVelocity;
                    forwardDir.y = 0; 
                    Vector3 bonusVelocity = forwardDir.normalized * (verticalSpeedBonus * fallSpeedToSlideBoostMultiplier);
                    rd.AddForce(bonusVelocity, ForceMode.VelocityChange); // 使用 VelocityChange 瞬间增加速度
                    LogUtil.Log($"应用了 {bonusVelocity.magnitude} 的落地滑铲速度增益");
                }
            }
            else
            {
                LogUtil.Log("速度不足，进入蹲伏状态");
                SwitchSubState<CrouchStatusStrategy>();
            }
            Ctx.MovementController.SetCrouchState(true);
        }

        
    }
}