using UnityEngine;

namespace UIManager
{
    public abstract class BaseHUDComponent:MonoBehaviour,IHUDComponent
    {
        protected bool _isHUDVisible = false;
        protected bool _isHUDEnabled = true;
        protected const bool _isDefaultHide = true;
        public virtual void RegisterMe()
        {
            var mainUiManager = MainUIManager.Instance;
            if (mainUiManager == null)
            {
                Debug.LogError($"{GetType().Name}: 找不到 MainUIManager，无法注册HUD组件");
                return;
            }
            MainUIManager.Instance.RegisterHUDComponent(this);
        }

        public void ShowHUD()
        {
           
        }

        public void HideHUD()
        {
            
        }

        public void UpdateHUDData(object data)
        {
           
        }

        public bool IsHUDVisible { get => _isHUDVisible; }
        public bool IsHUDEnabled { get => _isHUDEnabled; set=> _isHUDEnabled = value; }
        public bool IsDefaultHide { get=>_isDefaultHide; }
    }
}