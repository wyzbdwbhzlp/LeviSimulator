using DG.Tweening;
using TMPro;
using UIManager;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace HUD
{
    [HUD("InteractionHUD")]
    public class InteractionHUD : MonoBehaviour, IHUDComponent
    {
        [Header("UI组件")]
        [SerializeField] private GameObject hudContainer;
        [SerializeField] private TextMeshProUGUI interactionText;
        [SerializeField] private Image keyIcon;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("动画设置")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float pulseDuration = 1f;

        private bool _isVisible = false;
        private bool _isEnabled = true;
        private Tween _pulseTween;
        
        public bool IsDefaultHide=>true;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            // 初始化隐藏状态
            canvasGroup.alpha = 0f;
            hudContainer.SetActive(false);
            RegisterMe();
        }

        public void RegisterMe()
        {
            var mainUiManager = MainUIManager.Instance;
            if (mainUiManager == null)
            {
                Debug.LogError("InteractionHUD: 找不到 MainUIManager，无法注册HUD组件");
                return;
            }
            MainUIManager.Instance.RegisterHUDComponent(this);
        }

        [Button("显示HUD")]
        public void ShowHUD()
        {
            if (!_isEnabled || _isVisible) return;

            _isVisible = true;
            hudContainer.SetActive(true);

            // 淡入动画
            canvasGroup.DOFade(1f, fadeInDuration)
                .SetEase(Ease.OutQuad);

            // 开始脉动动画
            StartPulseAnimation();
        }

        [Button("隐藏HUD")]
        public void HideHUD()
        {
            if (!_isVisible) return;

            _isVisible = false;
            StopPulseAnimation();
            
            // 淡出动画
            canvasGroup.DOFade(0f, fadeOutDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => hudContainer.SetActive(false));
        }

        public void UpdateHUDData(object data)
        {
            if (!_isEnabled) return;

            if (data is InteractionData interactionData)
            {
                interactionText.text = interactionData.promptText;
                
                if (keyIcon != null && interactionData.keySprite != null)
                {
                    keyIcon.sprite = interactionData.keySprite;
                }
            }
            else if (data is string promptText)
            {
                interactionText.text = promptText;
            }
        }

        public bool IsHUDVisible => _isVisible;

        public bool IsHUDEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (!_isEnabled && _isVisible)
                {
                    HideHUD();
                }
            }
        }
        

        private void StartPulseAnimation()
        {
            if (keyIcon != null)
            {
                _pulseTween = keyIcon.transform
                    .DOScale(pulseScale, pulseDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }

        private void StopPulseAnimation()
        {
            _pulseTween?.Kill();
            if (keyIcon != null)
            {
                keyIcon.transform.localScale = Vector3.one;
            }
        }

        private void OnDestroy()
        {
            _pulseTween?.Kill();
        }
    }

    /// <summary>
    /// 交互数据结构
    /// </summary>
    [System.Serializable]
    public class InteractionData
    {
        public string promptText;
        public Sprite keySprite;

        public InteractionData(string text, Sprite sprite = null)
        {
            promptText = text;
            keySprite = sprite;
        }
    }
}