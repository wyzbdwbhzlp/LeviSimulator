namespace PlayerControllers.CalculationPhysicsComponents
{
    public class GrapplingHookPhysicsCalculationComponent:ICalculationPhysicsComponent
    {
        public void OnInit(PlayerMovementController movementController)
        {
            // 钩爪物理计算组件不需要初始化逻辑
            // 由 PlayerGrappleController 处理钩爪的物理计算
        }

        public void HandleMovementPhysics()
        {
            return; // 钩爪物理计算由 PlayerGrappleController 处理，不在此处实现
        }
    }
}