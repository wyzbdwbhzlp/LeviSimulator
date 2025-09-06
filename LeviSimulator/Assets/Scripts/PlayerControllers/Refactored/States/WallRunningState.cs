using UnityEngine;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Systems;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 滑墙状态
    /// </summary>
    public class WallRunningState : PlayerStateBase
    {
        private PlayerWallRunSystem _wallRunSystem;
        
        public WallRunningState(PlayerController controller) : base(controller) 
        {
            _wallRunSystem = controller.WallRunSystem;
        }
        
        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.WallRunning);
            runtimeData.SetWallRunning(true);
            
            if (_wallRunSystem != null)
            {
                _wallRunSystem.StartWallRun();
            }
        }
        
        public override void Exit()
        {
            base.Exit();
            runtimeData.SetWallRunning(false);
            
            if (_wallRunSystem != null)
            {
                _wallRunSystem.StopWallRun();
            }
        }
        
        protected override void CheckTransitions()
        {
            // 检查是否失去滑墙条件
            if (_wallRunSystem == null || !_wallRunSystem.CanWallRun)
            {
                ChangeState(PlayerState.Falling);
                return;
            }
            
            // 检查速度是否过低
            if (runtimeData.HorizontalSpeed < _wallRunSystem.WallRunMinimumSpeed)
            {
                ChangeState(PlayerState.Falling);
                return;
            }
            
            // 检查是否着地
            if (runtimeData.IsGrounded)
            {
                if (runtimeData.MoveInput.magnitude > 0.1f)
                {
                    if (runtimeData.IsSprinting)
                        ChangeState(PlayerState.Running);
                    else
                        ChangeState(PlayerState.Walking);
                }
                else
                {
                    ChangeState(PlayerState.Idle);
                }
                return;
            }
        }
        
        protected override void HandleMovement()
        {
            // 滑墙移动由WallRunSystem处理
            // 这里可以添加额外的移动逻辑，如重力抵消等
            
            // 抵消重力，保持在墙上
            Vector3 velocity = movementSystem.Rigidbody.linearVelocity;
            if (velocity.y < 0) // 只在下落时抵消重力
            {
                Vector3 gravityCounterForce = Vector3.up * (config.Gravity * 0.8f); // 轻微的重力抵消
                movementSystem.Rigidbody.AddForce(gravityCounterForce, ForceMode.Acceleration);
            }
        }
        
        public override bool CanTransitionTo(PlayerState targetState)
        {
            switch (targetState)
            {
                case PlayerState.Jumping:
                    return true; // 滑墙时可以跳跃（墙跳）
                case PlayerState.Falling:
                    return true; // 可以掉落
                case PlayerState.Idle:
                case PlayerState.Walking:
                case PlayerState.Running:
                    return runtimeData.IsGrounded; // 只有着地时才能转换到地面状态
                default:
                    return false;
            }
        }
    }
}
