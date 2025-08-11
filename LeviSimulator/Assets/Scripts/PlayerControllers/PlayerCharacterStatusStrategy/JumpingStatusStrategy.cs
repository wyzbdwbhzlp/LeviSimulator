using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class JumpingStatusStrategy:BaseStatusStrategy
    {
        private PlayerMovementController movementController;
        private PlayerWallRunController wallRunController;
        public override void HandleInput(PlayerInput input)
        {
            
        }

        public override void LogicUpdate()
        {
            if (wallRunController.CanWallRun()
                &&
                wallRunController.WallRunThresholdSpeed<= movementController.CurrentPlayerRdHorizontalVelocityMagnitude)
            {
                _playerInputRouter.ChangeStatus<WallRunningStatusStrategy>();
                return;
            }
            if (movementController.CurrentPlayerRdVelocity.y <= -1)
            {
                _playerInputRouter.ChangeStatus<FallingStatusStrategy>();
            }
        }

        public override void OnEnter(PlayerInputRouter input)
        {
            base.OnEnter(input);
            movementController = _playerInputRouter.MovementController;
            wallRunController = _playerInputRouter.PlayerWallRunController;
            
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
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
                    _playerInputRouter.PlayerGrappleController.StartGrapple();
                    LogUtil.Log("开始发射钩爪");
                    break;
                case InputActionPhase.Canceled:
                    _playerInputRouter.PlayerGrappleController.StopGrapple();
                    break;
            }
        }
    }
}