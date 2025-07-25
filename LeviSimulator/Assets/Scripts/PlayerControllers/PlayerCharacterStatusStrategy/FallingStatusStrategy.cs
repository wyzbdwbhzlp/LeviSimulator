using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class FallingStatusStrategy: IPlayerCharacterStatusStrategy
    {
        private PlayerInputRouter playerInputRouter;
        public void HandleInput(PlayerInput input)
        {
            
        }

        public void LogicUpdate()
        {
            if(playerInputRouter.MovementController.IsGrounded)
            {
                playerInputRouter.ChangeStatus(new WalkingStatusStrategy());
            }
           
        }

        public void OnEnter(PlayerInputRouter input)
        {
            playerInputRouter= input;
        }

        public void OnExit()
        {
           
        }

        public void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
           
        }
    }
}