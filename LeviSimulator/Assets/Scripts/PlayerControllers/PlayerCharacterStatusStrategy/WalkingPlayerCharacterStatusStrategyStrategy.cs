
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class WalkingStatusStrategyStrategy:IPlayerCharacterStatusStrategy
    {
        private PlayerInputRouter PlayerInputRouter;
        private Vector2 _currentMoveInput;
    
   
        public void HandleInput(PlayerInput input)
        {
           
        }

        public void LogicUpdate()
        {
            PlayerInputRouter.MovementController.ApplyMovement(new Vector3(_currentMoveInput.x, 0, _currentMoveInput.y));
        }

        public void OnEnter(PlayerInputRouter movementController)
        {
            PlayerInputRouter= movementController;
            
            PlayerInputRouter.PlayerInput.onActionTriggered+= HandleonActionTriggered;
            _currentMoveInput = Vector2.zero; 
        }
        
        public void OnExit()
        {
            PlayerInputRouter.PlayerInput.onActionTriggered-= HandleonActionTriggered;
            _currentMoveInput = Vector2.zero; 
        }
        public void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case "Move":
                    HandleMoveInput(obj);
                    break;
                case"LaunchGrapple":
                    HandleLaunchGrapple(obj);
                    break;
            }
            
        }

        private void HandleMoveInput(InputAction.CallbackContext callbackContext)
        {
            var moveInput= callbackContext.ReadValue<Vector2>();
            _currentMoveInput = moveInput;
        }
        private void HandleLaunchGrapple(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    PlayerInputRouter.PlayerGrappleController.StartGrapple();
                    LogUtil.Log("开始发射钩爪",true);
                    break;
                case InputActionPhase.Canceled:
                    PlayerInputRouter.PlayerGrappleController.StopGrapple();
                    break;
            }
           
        }

    }
}