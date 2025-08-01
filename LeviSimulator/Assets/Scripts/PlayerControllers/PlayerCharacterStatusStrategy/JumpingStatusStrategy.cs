using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class JumpingStatusStrategy:IPlayerCharacterStatusStrategy
    {
        PlayerInputRouter playerInputRouter;
        public void HandleInput(PlayerInput input)
        {
            
        }

        public void LogicUpdate()
        {
            if (playerInputRouter.MovementController.CurrentPlayerRdVelocity.y < 0)
            {
                playerInputRouter.ChangeStatus(new FallingStatusStrategy());
            }
        }

        public void OnEnter(PlayerInputRouter input)
        {
            playerInputRouter = input;
        }

        public void OnExit()
        {
            
        }

        public void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
        }
    }
}