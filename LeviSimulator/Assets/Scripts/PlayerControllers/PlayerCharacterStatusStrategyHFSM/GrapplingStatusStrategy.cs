using PlayerControllers.Grapple;
using PlayerControllers.PlayerCharacterStatusStrategy;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategyHFSM
{
    public class GrapplingStatusStrategy : HierarchicalBaseState 
    {
        private PlayerGrappleController _grappleController;
        private PlayerMovementController _movementController;

        protected override void EnterState()
        {
            _grappleController = Ctx.PlayerGrappleController;
            _movementController = Ctx.MovementController;
            EventBroadcaster.PlayerEndGrappleEvent+= OnEndGrapple;

            // 钩爪在切换到此状态之前已经由其他状态启动。
            // 这里我们确保物理状态适合钩爪摆动。
            _movementController.DisablePlayerRbGravity();
            LogUtil.Log("进入钩爪状态");
        }

        private void OnEndGrapple()
        {
            LogUtil.Log("钩爪结束事件触发，切换状态");
            if (_movementController.IsGrounded)
            {
                Ctx.ChangeParentStatus<GroundedState>();
            }
            else
            {
                Ctx.ChangeParentStatus<AirborneState>();
            }
        }

        protected override void ExitState()
        {
            EventBroadcaster.PlayerEndGrappleEvent -= OnEndGrapple;
            _movementController.EnablePlayerRbGravity();
            LogUtil.Log("退出钩爪状态");
        }

        protected override void UpdateStates()
        {
  
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
        }

        protected override bool HandleSubStateInput(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case "Jump":
                    if (obj.phase == InputActionPhase.Started)
                    {
                        Ctx.MovementController.StartJump();
                        SwitchSubState<JumpingStatusStrategy>();
                        return true; // 输入已处理
                    }
                    break;

                case "LaunchGrapple":
                    // 当玩家松开钩爪键时，停止钩爪
                    if (obj.phase == InputActionPhase.Canceled)
                    {
                        _grappleController.StopGrapple();
                        // 状态转换将在下一个 UpdateStates 中处理
                        return true; // 输入已处理
                    }
                    break;
            }
            return false; // 输入未处理
        }
    }
}