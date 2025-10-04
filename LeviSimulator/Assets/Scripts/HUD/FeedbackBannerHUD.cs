using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UIManager;
using UnityEngine;

namespace HUD
{
    [HUD("FeedbackBannerHUD")]
    public class FeedbackBannerHUD:MonoBehaviour,IHUDComponent
    {
        [SerializeField]private CanvasGroup canvasGroup;
        [SerializeField]private TextMeshProUGUI feedbackText;
        
        private void Awake()
        {
            canvasGroup.alpha = 0;
            RegisterMe();
        }

        private void RegisterMe()
        {
            var mainUiManager = MainUIManager.Instance;
            if (mainUiManager == null)
            {
                Debug.LogError("InteractionHUD: 找不到 MainUIManager，无法注册HUD组件");
                return;
            }
            MainUIManager.Instance.RegisterHUDComponent(this);
        }

        public void ShowHUD()
        {
            return;
            //无需手动实现
        }

        public void HideHUD()
        {
            return;
            //无需手动实现
        }

        public void UpdateHUDData(object data)
        {
            if (data is string str)
            {
                feedbackText.text = str;
                canvasGroup.alpha = 1;
                CancelInvoke(nameof(HideFeedback));
                Invoke(nameof(HideFeedback), 2f);
            }
            
        }
        private void HideFeedback()
        {
            canvasGroup.DOFade(0, 0.5f).Delay();
        }

        public bool IsHUDVisible { get; }
        public bool IsHUDEnabled { get; set; }
        public bool IsDefaultHide { get; }
    }
}