using UnityEngine.InputSystem;

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
        }

        public void OnExit()
        {
            
        }

        public void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
        }
    }
}