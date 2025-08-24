using PlayerControllers.PlayerCharacterStatusStrategyHFSM;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public abstract class BaseSubState
    {
        protected HierarchicalBaseState _currentSuperState;
        protected PlayerInputRouter Ctx;

        public virtual void EnterState(HierarchicalBaseState superState, PlayerInputRouter ctx)
        {
            _currentSuperState = superState;
            Ctx = ctx;
        }

        // 子类重写以实现自己的退出逻辑
        public abstract void ExitState();
        // 子类重写以实现自己的更新逻辑（状态切换检查）
        public abstract void UpdateStates();
        public abstract bool HandleInput(UnityEngine.InputSystem.InputAction.CallbackContext obj);
        
    }
}