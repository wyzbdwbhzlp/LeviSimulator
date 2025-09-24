using Game.Audio;
using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 行走状态
    /// </summary>
    public class WalkingState : PlayerStateBase
    {
        public WalkingState(PlayerController controller) : base(controller) { }
        private AudioSourceWrapper _audioSourceWrapper;
        
        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.Walking);
            _audioSourceWrapper=AudioEventHandler.CallPlayOneShotFor2D(AudioNames.走路声);
        }
        public override void Exit()
        {
            base.Exit();
            _audioSourceWrapper?.Stop();
        }

        protected override void CheckTransitions()
        {
            if (runtimeData.IsDashing)
            {
                ChangeState(PlayerState.Dashing);
                return;
            }

            // 检查是否停止移动
            if (runtimeData.MoveInput.magnitude < 0.1f)
            {
                ChangeState(PlayerState.Idle);
                return;
            }
            
            // 检查是否开始跑步
            if (runtimeData.IsSprinting)
            {
                ChangeState(PlayerState.Running);
                return;
            }
            
            // 检查是否蹲伏
            if (runtimeData.IsCrouching)
            {
                ChangeState(PlayerState.Crouching);
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
    }
}
