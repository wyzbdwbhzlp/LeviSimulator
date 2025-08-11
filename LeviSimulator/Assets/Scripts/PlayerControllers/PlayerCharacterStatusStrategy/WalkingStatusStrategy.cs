
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class WalkingStatusStrategy:BaseStatusStrategy
    {
    
   
        public override void HandleInput(PlayerInput input)
        {
           
        }

        public override void LogicUpdate()
        {
            if (!_playerInputRouter.MovementController.IsGrounded)
            {
                _playerInputRouter.ChangeStatus<FallingStatusStrategy>();
            }
        }

        public override void OnEnter(PlayerInputRouter inputRouter)
        {
            base.OnEnter(inputRouter);
            
        }
        
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case"LaunchGrapple":
                    HandleLaunchGrapple(obj);
                    break;
                case"Jump":
                    HandleJumpInput(obj);
                    break;
                case"Slide":
                    HandleSlideInput(obj);
                    LogUtil.Log("开始滑行");
                    break;
                case"Sprint":
                    HandleSprintInput(obj);
                    break;
            }
            
        }

        private void HandleSprintInput(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    _playerInputRouter.MovementController.TrySprint();
                    break;
                case InputActionPhase.Performed:
                    _playerInputRouter.MovementController.TrySprint();
                    break;
                default:
                    _playerInputRouter.MovementController.StopSprint();
                    break;
            }
        }

        private void HandleSlideInput(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    //TODO PlayerInputRouter.MovementController.StartSlide();
                    LogUtil.Log("尝试滑铲");
                    break;
            }
        }

        private void HandleJumpInput(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    _playerInputRouter.MovementController.StartJump();
                    LogUtil.Log("开始跳跃");
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