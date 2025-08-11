using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class GrapplingStatusStrategy : BaseStatusStrategy
    {
        public override void HandleInput(PlayerInput input)
        {
          
        }

        public override void LogicUpdate()
        {
         
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
            _playerInputRouter.MovementController.SetPlayerMovementTendency(new Vector3(moveInput.x, 0f, moveInput.y));
        }
    }
}