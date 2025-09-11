using Manager;
using UnityEngine;

namespace UIManager
{
    public abstract class BaseUIComponent:MonoBehaviour,IUIComponent
    {
        protected bool _isActive = false;

       

        public void ShowUIPanel(object data)
        {
           
        }

        public void CloseUIPanel()
        {
            
        }

        public bool IsUIComponentActive { get=>_isActive; }
    }
}