using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class FallingStatusStrategy: IPlayerCharacterStatusStrategy
    {
        private PlayerInputRouter playerInputRouter;
        private PlayerWallRunController playerWallRunController;
        private float extraFallForce = 3f;
        private float maxFallSpeed = 8f;
        public void HandleInput(PlayerInput input)
        {
            
        }

        public void LogicUpdate()
        {
            var movementController = playerInputRouter.MovementController;
            if (playerWallRunController.CanWallRun()&&playerWallRunController.WallRunThresholdSpeed<= movementController.CurrentPlayerRdHorizontalVelocityMagnitude)
            {
                playerInputRouter.ChangeStatus(new WallRunningStatusStrategy());
                return;
            }
            if (movementController.IsGrounded)
            {
                playerInputRouter.ChangeStatus(new WalkingStatusStrategy());
            }
            
            else
            {
                float currentFallSpeed = -playerInputRouter.MovementController.PlayerRigidbody.linearVelocity.y;
                
                if (currentFallSpeed < maxFallSpeed)
                {
                    float speedRatio = currentFallSpeed / maxFallSpeed;
                    float forceModifier = 1f - speedRatio;
                    Vector3 forceToApply = Vector3.down * extraFallForce * forceModifier;
                    playerInputRouter.MovementController.PlayerRigidbody.AddForce(forceToApply, ForceMode.Acceleration);
                }
            }
           
        }

        public void OnEnter(PlayerInputRouter input)
        {
            playerInputRouter= input;
            playerWallRunController = playerInputRouter.PlayerWallRunController;
            playerInputRouter.PlayerInput.onActionTriggered+= HandleonActionTriggered;
            LogUtil.Log("进入Falling状态，开始给予玩家额外的向下重力");
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