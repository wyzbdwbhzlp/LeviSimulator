using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public abstract class BaseStatusStrategy:IPlayerCharacterStatusStrategy
    {
        protected PlayerInputRouter _playerInputRouter;
    

        public virtual void LogicUpdate()
        {
            // 可以在这里添加一些通用的逻辑更新代码
        }

        public virtual void OnEnter(PlayerInputRouter input)
        {
            _playerInputRouter = input;
            _playerInputRouter.PlayerInput.onActionTriggered += HandleonActionTriggered;

        }

        public virtual void OnExit()
        {
            _playerInputRouter.PlayerInput.onActionTriggered -= HandleonActionTriggered;
            LogUtil.Log($"退出{GetType().Name}状态");
           
        }

        public virtual void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
        }
    }
}