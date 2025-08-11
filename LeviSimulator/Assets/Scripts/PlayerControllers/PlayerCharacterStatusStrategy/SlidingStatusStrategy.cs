using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class SlidingStatusStrategy:BaseStatusStrategy
    {
        private PlayerMovementController MovementController => _playerInputRouter.MovementController;
        public override void HandleInput(PlayerInput input)
        {
          
        }

        public override void LogicUpdate()
        {
            if (MovementController.MinSlideSpeed>MovementController.CurrentPlayerRdHorizontalVelocityMagnitude&&MovementController.IsGrounded)
            {
                _playerInputRouter.ChangeStatus<CrouchStatusStrategy>();
            }
          
        }

        public override void OnEnter(PlayerInputRouter input)
        {
            base.OnEnter(input);
            _playerInputRouter.MovementController.ResetPlayerMovementTendency();
            
           
        }

        public override void OnExit()
        {
            base.OnExit();

            
        }

        public override void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
          
        }
    }
}