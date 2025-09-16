using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 站立状态
    /// </summary>
    public class IdleState : PlayerStateBase
    {
        public IdleState(PlayerController controller) : base(controller) { }
        
        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.Idle);
            movementSystem.ResetCollider();
        }
        
        protected override void CheckTransitions()
        {
            if (runtimeData.IsDashing)
            {
                ChangeState(PlayerState.Dashing);
                return;
            }

            // 检查是否开始移动
            if (runtimeData.MoveInput.magnitude > 0.1f)
            {
                if (runtimeData.IsSprinting)
                    ChangeState(PlayerState.Running);
                else
                    ChangeState(PlayerState.Walking);
                return;
            }
            
            if (runtimeData.IsCrouching)
            {
                ChangeState(PlayerState.Crouching);
                return;
            }
            
            // 检查是否跳跃
            if (!runtimeData.IsGrounded)
            {
                ChangeState(PlayerState.Falling);
                return;
            }
        }
        
        protected override void HandleMovement()
        {
            // 在静止状态应用减速
            if (runtimeData.HorizontalSpeed > 0.1f)
            {
                Vector3 velocity = movementSystem.Rigidbody.linearVelocity;
                velocity.x = Mathf.MoveTowards(velocity.x, 0, config.Deceleration * Time.fixedDeltaTime);
                velocity.z = Mathf.MoveTowards(velocity.z, 0, config.Deceleration * Time.fixedDeltaTime);
                movementSystem.Rigidbody.linearVelocity = velocity;
            }
        }
    }
}
