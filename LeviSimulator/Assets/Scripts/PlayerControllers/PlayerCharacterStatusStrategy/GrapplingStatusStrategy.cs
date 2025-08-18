using PlayerControllers.Grapple;
using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class GrapplingStatusStrategy : HierarchicalBaseState
    {
        private PlayerGrappleController _grappleController;
        private PlayerMovementController _movementController;

        protected override void EnterState()
        {
            _grappleController = Ctx.PlayerGrappleController;
            _movementController = Ctx.MovementController;

            // 钩爪在切换到此状态之前已经由其他状态启动。
            // 这里我们确保物理状态适合钩爪摆动。
            _movementController.DisablePlayerRbGravity();
            LogUtil.Log("进入钩爪状态");
        }

        protected override void ExitState()
        {
            // 确保钩爪已停止并且物理状态已重置。
            if (_grappleController.IsGrappling)
            {
                _grappleController.StopGrapple();
            }
            _movementController.EnablePlayerRbGravity();
            LogUtil.Log("退出钩爪状态");
        }

        protected override void UpdateStates()
        {
            // 如果钩爪因任何原因（例如到达目的地）停止，则转换状态。
            if (!_grappleController.IsGrappling)
            {
                if (_movementController.IsGrounded)
                {
                    SwitchSubState<WalkingStatusStrategy>();
                }
                else
                {
                    SwitchSubState<FallingStatusStrategy>();
                }
            }
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
            // GrapplingStatusStrategy 不直接处理顶层输入，
            // 输入逻辑在子状态或 HandleSubStateInput 中处理
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