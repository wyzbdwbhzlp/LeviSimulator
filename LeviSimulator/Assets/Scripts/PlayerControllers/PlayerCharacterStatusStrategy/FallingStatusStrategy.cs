using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class FallingStatusStrategy : HierarchicalBaseState
    {
        private PlayerMovementController _movementController;
        private PlayerWallRunController _wallRunController;

        protected override void EnterState()
        {
            _movementController = Ctx.MovementController;
            _wallRunController = Ctx.PlayerWallRunController;
            LogUtil.Log("进入下落状态");
        }

        protected override void ExitState()
        {
            // 此状态没有特定的退出逻辑
        }

        protected override void UpdateStates()
        {
            // 检查是否可以滑墙
            if (_wallRunController.CanWallRun() && 
                _movementController.CurrentPlayerRdHorizontalVelocityMagnitude >= _wallRunController.WallRunThresholdSpeed)
            {
                SwitchSubState<WallRunningStatusStrategy>();
                return;
            }
            
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
            // 处理下落时的输入，例如发射钩爪
            switch (obj.action.name)
            {
                case "LaunchGrapple":
                    HandleLaunchGrapple(obj);
                    break;
            }
        }

        private void HandleLaunchGrapple(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    Ctx.PlayerGrappleController.StartGrapple();
                    LogUtil.Log("开始发射钩爪");
                    break;
                case InputActionPhase.Canceled:
                    Ctx.PlayerGrappleController.StopGrapple();
                    break;
            }
        }
    }
}