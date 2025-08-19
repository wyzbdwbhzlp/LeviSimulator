using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class WalkingStatusStrategy : HierarchicalBaseState
    {
        protected override void EnterState() { }
        protected override void ExitState() 
        {
        }

        protected override void UpdateStates()
        {
        }

       
        protected override bool HandleSubStateInput(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case "Crouch":
                    if(obj.phase == InputActionPhase.Started)
                    {
                        StartCrouchOrSliding();
                        return true; // 已处理
                    }
                    break;
                case "Sprint":
                    switch (obj.phase)
                    {
                        case InputActionPhase.Performed:
                            Ctx.MovementController.TrySprint();
                            break;
                        default:
                            Ctx.MovementController.StopSprint();
                            break;
                    }
                    return true; 
            }
            return false; 
        }
        
        protected override void HandleInput(InputAction.CallbackContext obj) { }
    }
}