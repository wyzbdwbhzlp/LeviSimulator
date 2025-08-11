using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class FallingStatusStrategy: BaseStatusStrategy
    {
       
        private PlayerWallRunController playerWallRunController;
        private float extraFallForce = 3f;
        private float maxFallSpeed = 8f;
        public override void HandleInput(PlayerInput input)
        {
            
        }

        public override void LogicUpdate()
        {
            var movementController = _playerInputRouter.MovementController;
            if (playerWallRunController.CanWallRun()&&playerWallRunController.WallRunThresholdSpeed<= movementController.CurrentPlayerRdHorizontalVelocityMagnitude)
            {
                _playerInputRouter.ChangeStatus<WallRunningStatusStrategy>();
                return;
            }
            if (movementController.IsGrounded)
            {
                _playerInputRouter.ChangeStatus<WalkingStatusStrategy>();
            }
            
            else
            {
                float currentFallSpeed = -_playerInputRouter.MovementController.PlayerRigidbody.linearVelocity.y;
                
                if (currentFallSpeed < maxFallSpeed)
                {
                    float speedRatio = currentFallSpeed / maxFallSpeed;
                    float forceModifier = 1f - speedRatio;
                    Vector3 forceToApply = Vector3.down * extraFallForce * forceModifier;
                    _playerInputRouter.MovementController.PlayerRigidbody.AddForce(forceToApply, ForceMode.Acceleration);
                }
            }
           
        }

        public override void OnEnter(PlayerInputRouter input)
        {
            base.OnEnter(input);
            playerWallRunController = _playerInputRouter.PlayerWallRunController;
            LogUtil.Log("进入Falling状态，开始给予玩家额外的向下重力");
        }

        public override void OnExit()
        {
            base.OnExit();
            
           
        }

        public override void HandleonActionTriggered(InputAction.CallbackContext obj)
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
                    _playerInputRouter.PlayerGrappleController.StartGrapple();
                    LogUtil.Log("开始发射钩爪");
                    break;
                case InputActionPhase.Canceled:
                    _playerInputRouter.PlayerGrappleController.StopGrapple();
                    break;
            }
        }
    }
}