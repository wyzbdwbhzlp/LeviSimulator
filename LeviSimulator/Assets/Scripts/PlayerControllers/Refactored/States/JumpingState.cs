using Game.Audio;
using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 跳跃状态
    /// </summary>
    public class JumpingState : PlayerStateBase
    {
        private float _jumpStartTime;
        
        public JumpingState(PlayerController controller) : base(controller) { }
        private AudioSourceWrapper _audioSourceWrapper;
        
        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.Jumping);
            
            // 应用跳跃力
            movementSystem.ApplyJump(config.JumpForce);
            
            runtimeData.SetJumping(true);
            
            _jumpStartTime = Time.time;
            
            _audioSourceWrapper=AudioEventHandler.CallPlayOneShotFor2D(AudioNames.女跳跃A新版);
        }
        
        public override void Exit()
        {
            base.Exit();

            _audioSourceWrapper.DelayStop(0.3f); 
            
            runtimeData.SetJumping(false);
        }
        
        protected override void CheckTransitions()
        {
            if (runtimeData.IsDashing)
            {
                ChangeState(PlayerState.Dashing);
                return;
            }

            // 如果开始下落，转换到下落状态
            if (movementSystem.Rigidbody.linearVelocity.y <= 0)
            {
                ChangeState(PlayerState.Falling);
                return;
            }
            
            // 检查是否着地
            if (runtimeData.IsGrounded && Time.time - _jumpStartTime > 0.1f) // 防止立即检测到地面
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
            ApplyAirMovement();
        }
        
        public override bool CanTransitionTo(PlayerState targetState)
        {
            // 跳跃状态不能直接转换到某些状态
            switch (targetState)
            {
                case PlayerState.Sliding:
                case PlayerState.Crouching:
                    return false;
                default:
                    return true;
            }
        }
    }
}
