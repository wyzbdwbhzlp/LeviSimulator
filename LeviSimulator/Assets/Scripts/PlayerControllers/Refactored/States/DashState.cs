using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.States
{
    public class DashState: PlayerStateBase
    {
        public DashState(PlayerController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.Dashing);
            playerController.MovementSystem.DisablePlayerGravity();
            // 启动冲刺逻辑，例如设置速度、播放动画等
        }

        public override void Exit()
        {
            base.Exit();
            playerController.MovementSystem.EnablePlayerGravity();
            // 清理冲刺状态，例如重置速度等
        }

        protected override void CheckTransitions()
        {
            if (!runtimeData.IsDashing)
            {
                if (playerController.RuntimeData.IsGrounded)
                {
                    ChangeState(PlayerState.Idle);
                    return;
                }
                else
                {
                    ChangeState(PlayerState.Falling);
                    return;
                }
            }
        }

        protected override void HandleMovement()
        {
            // 实现冲刺时的移动逻辑
        }
    }
}