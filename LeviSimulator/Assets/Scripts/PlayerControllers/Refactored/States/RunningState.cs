using Game.Audio;
using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 跑步状态
    /// </summary>
    public class RunningState : PlayerStateBase
    {
        public RunningState(PlayerController controller) : base(controller) { }
        private AudioSourceWrapper _audioSourceWrapper;
     
        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.Running);
            _audioSourceWrapper=AudioEventHandler.CallPlayOneShotFor2D(AudioNames.跑步声);
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
            
            // 检查是否停止跑步
            if (!runtimeData.IsSprinting)
            {
                ChangeState(PlayerState.Walking);
                return;
            }
            
            // 检查是否蹲伏（跑步时蹲伏转为滑铲）
            if (runtimeData.IsCrouching && runtimeData.HorizontalSpeed > config.MinSlideActivationSpeed)
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
    }
}
