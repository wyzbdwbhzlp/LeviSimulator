using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

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
            playerInputRouter.MovementController.PlayerRigidbody.AddForce(Vector3.down*9f, ForceMode.Acceleration);//TODO 写死并不是好事
            LogUtil.Log("进入Falling状态，给予玩家额外的向下重力");
        }

        public void OnExit()
        {
           
        }

        public void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
           
        }
    }
}