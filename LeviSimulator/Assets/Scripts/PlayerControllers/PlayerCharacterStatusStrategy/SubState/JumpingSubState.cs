using PlayerControllers.Grapple;
using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategy.SubState
{
    public class JumpingSubState:BaseSubState
    {
        private PlayerMovementController _movementController;
        private PlayerWallRunController _wallRunController;
        private PlayerGrappleController _grappleController;
        public override void EnterState(HierarchicalBaseState superState, PlayerInputRouter ctx)
        {
            base.EnterState(superState, ctx);
            _movementController = Ctx.MovementController;
            _wallRunController = Ctx.PlayerWallRunController;
            _grappleController = Ctx.PlayerGrappleController;
        }
        public override void ExitState()
        {
        }

        public override void UpdateStates()
        {
            if (_movementController.PlayerRigidbody.linearVelocity.y <= 0)
            {
                _currentSuperState.SwitchSubState<FallingSubState>();
                return;
            }

            // 检查是否可以滑墙
            if (_wallRunController.CanWallRun() && 
                _movementController.CurrentPlayerRdHorizontalVelocityMagnitude >= _wallRunController.WallRunThresholdSpeed)
            {
                _currentSuperState.SwitchSubState<WallRunningSubState>();
            }
        }

        public override bool HandleInput(InputAction.CallbackContext obj)
        {
            if (obj.action.name == "LaunchGrapple")
            {
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