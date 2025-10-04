using System;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Sirenix.OdinInspector;
using UIManager;


namespace CollectibleEcho
{
    [ViewComponent("CollectibleEchoCanvas")]
    public class EchoDisplayViewUI:Singleton<EchoDisplayViewUI>,IViewComponent
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private Image illustrationImage;
        [SerializeField] private Button closeButton;
        private bool _isUIComponentActive = true;
        public Button CloseButton => closeButton;
        protected override void Awake()
        {
            base.Awake();
        }

        protected void Start()
        {
            MainUIManager.Instance.RegisterViewComponent(this);
            
        }

        /// <summary>
        ///  显示Echo
        /// </summary>
        /// <param name="data"></param>
        public void ShowUIPanel(object data)
        {
            if (data is not SO.CollectibleEcho echoData)
            {
                LogUtil.LogError("传入的data不是EchoData类型或data为null", true);
                return;
            }
            
            titleText.text = echoData.EchoTitle;
            contentText.text = echoData.EchoText;
            SetAnchorTopStretch();
            var illustration = echoData.GetEchoIllustrationsSprite();
            if (illustration != null)
            {
                illustrationImage.sprite = illustration;
                illustrationImage.gameObject.SetActive(true);
            }
            else
            {
                illustrationImage.gameObject.SetActive(false);
            }
            _isUIComponentActive = true;
        }
        public void CloseUIPanel()
        {
            titleText.text = string.Empty;
            contentText.text = string.Empty;
            illustrationImage.sprite = null;
            illustrationImage.gameObject.SetActive(false);
            _isUIComponentActive = false;
        }

        public bool IsUIComponentActive => _isUIComponentActive;


        [Button]
        public void SetAnchorTopStretch()
        {
            var rt = contentText.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            
            rt.offsetMax = new Vector2(0f, 0f);     
        }
    }
}