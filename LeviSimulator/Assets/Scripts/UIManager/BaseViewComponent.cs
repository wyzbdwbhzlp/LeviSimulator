using Manager;
using UnityEngine;

namespace UIManager
{
    public abstract class BaseViewComponent:MonoBehaviour,IViewComponent
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