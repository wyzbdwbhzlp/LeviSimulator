
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class WalkingStatusStrategy:IPlayerCharacterStatusStrategy
    {
        private PlayerInputRouter PlayerInputRouter;
    
   
        public void HandleInput(PlayerInput input)
        {
           
        }

        public void LogicUpdate()
        {
            if (!PlayerInputRouter.MovementController.IsGrounded)
            {
                PlayerInputRouter.ChangeStatus(new FallingStatusStrategy());
            }
        }

        public void OnEnter(PlayerInputRouter InputRouter)
        {
            PlayerInputRouter= InputRouter;
            PlayerInputRouter.PlayerInput.onActionTriggered+= HandleonActionTriggered;
            
        }
        
        public void OnExit()
        {
            PlayerInputRouter.PlayerInput.onActionTriggered-= HandleonActionTriggered;
            LogUtil.Log("退出Walking状态");
        }
        public void HandleonActionTriggered(InputAction.CallbackContext obj)
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
                case InputActionPhase.Performed:
                    PlayerInputRouter.MovementController.TrySprint();
                    break;
                case InputActionPhase.Canceled:
                    PlayerInputRouter.MovementController.StopSprint();
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
                    PlayerInputRouter.MovementController.StartJump();
                    LogUtil.Log("开始跳跃");
                    break;
            }
        }
        
        private void HandleLaunchGrapple(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    PlayerInputRouter.PlayerGrappleController.StartGrapple();
                    LogUtil.Log("开始发射钩爪");
                    break;
                case InputActionPhase.Canceled:
                    PlayerInputRouter.PlayerGrappleController.StopGrapple();
                    break;
            }
           
        }

    }
}