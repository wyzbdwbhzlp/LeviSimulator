using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 蹲伏状态
    /// </summary>
    public class CrouchingState : PlayerStateBase
    {
        public CrouchingState(PlayerController controller) : base(controller) { }
        
        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.Crouching);
            runtimeData.SetCrouching(true);
            
            // 降低碰撞体高度
            movementSystem.SetColliderHeight(config.CrouchingHeight);
        }
        
        public override void Exit()
        {
            base.Exit();
            runtimeData.SetCrouching(false);
            
            // 检查是否可以站起
            if (!movementSystem.CheckCeiling())
            {
                movementSystem.ResetCollider();
            }
        }
        
        protected override void CheckTransitions()
        {
            // 检查是否停止蹲伏
            if (!runtimeData.IsCrouching)
            {
                // 检查头顶是否有障碍物
                if (!movementSystem.CheckCeiling())
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
                }
                else
                {
                    // 如果头顶有障碍物，继续蹲伏
                    runtimeData.SetCrouching(true);
                }
                return;
            }
            
            // 检查是否开始滑铲
            if (runtimeData.HorizontalSpeed > config.MinSlideActivationSpeed && runtimeData.MoveInput.magnitude > 0.1f)
            {
                ChangeState(PlayerState.Sliding);
                return;
            }
            
            // 检查是否离开地面
            if (!runtimeData.IsGrounded)
            {
                ChangeState(PlayerState.Falling);
                return;
            }
        }
        
        protected override void HandleMovement()
        {
            ApplyGroundMovement();
        }
        
        public override bool CanTransitionTo(PlayerState targetState)
        {
            switch (targetState)
            {
                case PlayerState.Jumping:
                    return false; // 蹲伏时不能跳跃
                default:
                    return true;
            }
        }
    }
}
