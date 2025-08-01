using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class GrapplingStatusStrategy : IPlayerCharacterStatusStrategy
    {
        private PlayerInputRouter playerInputRouter;
        public void HandleInput(PlayerInput input)
        {
          
        }

        public void LogicUpdate()
        {
         
        }

        public void OnEnter(PlayerInputRouter input)
        {
            playerInputRouter = input;
            playerInputRouter.MovementController.ResetPlayerMovementTendency();
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
                case "Move":
                    HandleMoveInput(obj);
                    break;
            }
        }

        private void HandleMoveInput(InputAction.CallbackContext callbackContext)
        {
            var moveInput= callbackContext.ReadValue<Vector2>();
            LogUtil.Log($"Grappling状态下的移动输入: {moveInput}");
            playerInputRouter.MovementController.SetPlayerMovementTendency(new Vector3(moveInput.x, 0f, moveInput.y));
        }
    }
}