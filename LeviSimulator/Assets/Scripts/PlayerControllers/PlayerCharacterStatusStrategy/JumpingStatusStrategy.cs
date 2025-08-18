using PlayerControllers.Grapple;
using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class JumpingStatusStrategy : HierarchicalBaseState
    {
        private PlayerMovementController _movementController;
        private PlayerWallRunController _wallRunController;
        private PlayerGrappleController _grappleController;

        protected override void EnterState()
        {
            _movementController = Ctx.MovementController;
            _wallRunController = Ctx.PlayerWallRunController;
            _grappleController = Ctx.PlayerGrappleController;
            LogUtil.Log("进入跳跃状态");
        }

        protected override void ExitState()
        {
            // 此状态没有特定的退出逻辑
        }

        protected override void UpdateStates()
        {
            // 当上升速度消失时，切换到下落状态
            if (_movementController.PlayerRigidbody.linearVelocity.y <= 0)
            {
                SwitchSubState<FallingStatusStrategy>();
                return;
            }

            // 检查是否可以滑墙
            if (_wallRunController.CanWallRun() && 
                _movementController.CurrentPlayerRdHorizontalVelocityMagnitude >= _wallRunController.WallRunThresholdSpeed)
            {
                SwitchSubState<WallRunningStatusStrategy>();
            }
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
            // 处理跳跃时的输入，例如发射钩爪
            if (obj.action.name == "LaunchGrapple")
            {
                HandleLaunchGrapple(obj);
            }
        }

        private void HandleLaunchGrapple(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    _grappleController.StartGrapple();
                    // StartGrapple 成功后会通过事件或控制器逻辑切换到 GrapplingStatusStrategy
                    break;
                case InputActionPhase.Canceled:
                    _grappleController.StopGrapple();
                    break;
            }
        }
    }
}