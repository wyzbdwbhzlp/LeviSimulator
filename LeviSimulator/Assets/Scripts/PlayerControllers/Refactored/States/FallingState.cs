using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 下落状态
    /// </summary>
    public class FallingState : PlayerStateBase
    {
        public FallingState(PlayerController controller) : base(controller) { }
        
        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.Falling);
        }
        
        protected override void CheckTransitions()
        {
            if (runtimeData.IsDashing)
            {
                ChangeState(PlayerState.Dashing);
                return;
            }

            // 检查是否可以开始滑墙
            var wallRunSystem = playerController.WallRunSystem;
            if (wallRunSystem != null && wallRunSystem.CanWallRun && 
                runtimeData.HorizontalSpeed >= wallRunSystem.WallRunThresholdSpeed)
            {
                ChangeState(PlayerState.WallRunning);
                return;
            }
            
            // 检查是否着地
            if (runtimeData.IsGrounded)
            {
                // 根据着地时的速度和输入决定下一个状态
                if (runtimeData.IsCrouching && runtimeData.HorizontalSpeed > config.MinSlideSpeed)
                {
                    ChangeState(PlayerState.Sliding);
                    return;
                }
                
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
            ApplyAirMovement();
            
            // 应用重力
            Vector3 velocity = movementSystem.Rigidbody.linearVelocity;
            velocity.y -= config.Gravity * Time.fixedDeltaTime;
            movementSystem.Rigidbody.linearVelocity = velocity;
        }
    }
}
