using UnityEngine;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;
using PlayerControllers.Refactored.Systems;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 状态基类
    /// </summary>
    public abstract class PlayerStateBase : IState
    {
        protected PlayerController playerController;
        protected PlayerRuntimeData runtimeData;
        protected PlayerMovementSystem movementSystem;
        protected PlayerMovementConfig config;
        
        public PlayerStateBase(PlayerController controller)
        {
            playerController = controller;
            runtimeData = controller.RuntimeData;
            movementSystem = controller.MovementSystem;
            config = movementSystem.Config;
        }
        
        public virtual void Enter()
        {
            Debug.Log($"Entering {GetType().Name}");
        }
        
        public virtual void Update()
        {
            CheckTransitions();
        }
        
        public virtual void FixedUpdate()
        {
            HandleMovement();
        }
        
        public virtual void Exit()
        {
            Debug.Log($"Exiting {GetType().Name}");
        }
        
        public virtual bool CanTransitionTo(PlayerState targetState)
        {
            return true; // 默认允许所有转换
        }

        public virtual void OnJumpPressed()
        {
            if (runtimeData.IsGrounded&&runtimeData.CanJump && playerController.StateMachine.CurrentState != PlayerState.Crouching)
            {
                ChangeState(PlayerState.Jumping);
            }
        }

        /// <summary>
        /// 检查状态转换条件
        /// </summary>
        protected abstract void CheckTransitions();
        
        /// <summary>
        /// 处理移动逻辑
        /// </summary>
        protected abstract void HandleMovement();
        
        /// <summary>
        /// 改变状态的辅助方法
        /// </summary>
        protected void ChangeState(PlayerState newState)
        {
            playerController.StateMachine.ChangeState(newState);
        }
        
        /// <summary>
        /// 获取目标移动速度
        /// </summary>
        protected float GetTargetSpeed()
        {
            if (runtimeData.MoveInput.magnitude < 0.1f)
                return 0f;
                
            if (runtimeData.IsCrouching)
                return config.CrouchSpeed;
                
            if (runtimeData.IsSprinting)
                return config.RunSpeed;
                
            return config.WalkSpeed;
        }
        
        /// <summary>
        /// 应用基础地面移动
        /// </summary>
        protected void ApplyGroundMovement()
        {
            Vector3 moveDirection = runtimeData.MoveDirection;
            float targetSpeed = GetTargetSpeed();
            
            Vector3 targetVelocity = moveDirection * targetSpeed;
            float acceleration = moveDirection.magnitude > 0.1f ? config.Acceleration : config.Deceleration;
            
            movementSystem.ApplyMovement(targetVelocity, acceleration);
        }
        
        /// <summary>
        /// 应用空中移动
        /// </summary>
        protected void ApplyAirMovement()
        {
            Vector3 moveDirection = runtimeData.MoveDirection;
            float targetSpeed = Mathf.Min(GetTargetSpeed(), config.MaxAirSpeed);
            
            Vector3 targetVelocity = moveDirection * targetSpeed;
            movementSystem.ApplyMovement(targetVelocity, config.AirAcceleration);
        }
    }
}
