using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class JumpingStatusStrategy:IPlayerCharacterStatusStrategy
    {
        private PlayerInputRouter playerInputRouter;
        private PlayerMovementController movementController;
        private PlayerWallRunController wallRunController;
        public void HandleInput(PlayerInput input)
        {
            
        }

        public void LogicUpdate()
        {
            if (wallRunController.CanWallRun()
                &&
                wallRunController.WallRunThresholdSpeed<= movementController.CurrentPlayerRdHorizontalVelocityMagnitude)
            {
                playerInputRouter.ChangeStatus(new WallRunningStatusStrategy());
                return;
            }
            if (movementController.CurrentPlayerRdVelocity.y <= -1)
            {
                playerInputRouter.ChangeStatus(new FallingStatusStrategy());
            }
        }

        public void OnEnter(PlayerInputRouter input)
        {
            playerInputRouter = input;
            movementController = playerInputRouter.MovementController;
            wallRunController = playerInputRouter.PlayerWallRunController;
            playerInputRouter.PlayerInput.onActionTriggered+= HandleonActionTriggered;
        }

        public void OnExit()
        {
            playerInputRouter.PlayerInput.onActionTriggered-= HandleonActionTriggered;
        }

        public void HandleonActionTriggered(InputAction.CallbackContext obj)
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
                    playerInputRouter.PlayerGrappleController.StartGrapple();
                    LogUtil.Log("开始发射钩爪");
                    break;
                case InputActionPhase.Canceled:
                    playerInputRouter.PlayerGrappleController.StopGrapple();
                    break;
            }
        }
    }
}