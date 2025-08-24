using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy.SubState
{
    public class FallingSubState:BaseSubState
    {
        private PlayerMovementController _movementController;
        private PlayerWallRunController _wallRunController;
        public override void EnterState(HierarchicalBaseState superState, PlayerInputRouter ctx)
        {
            base.EnterState(superState, ctx);
            _movementController = Ctx.MovementController;
            _wallRunController = Ctx.PlayerWallRunController;
        }

        public override void ExitState()
        {
           
        }

        public override void UpdateStates()
        {
            if (!_wallRunController.IsWallRunning &&_wallRunController.IsCanWallRun &&
                _movementController.CurrentPlayerRdHorizontalVelocityMagnitude >= _wallRunController.WallRunThresholdSpeed)
            {
                _currentSuperState.SwitchSubState<WallRunningSubState>();
                return;
            }
        }

        public override bool HandleInput(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case "LaunchGrapple":
                    HandleLaunchGrapple(obj);
                    return true;
            }
            return false;
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