using System.Collections.Generic;
using DG.Tweening;
using PlayerControllers.Refactored.Data;
using PlayerControllers.Refactored.Systems;
using Sirenix.OdinInspector;
using UIManager;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Random = UnityEngine.Random;

namespace HUD
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Image))]
    [HUD("CameraSpeedLineHUD")]
    public class CameraSpeedLineHUD : MonoBehaviour,IHUDComponent
    {
        [SerializeField] private List<Sprite> speedLineSprites = new();
        [SerializeField] private float speedLineFadeInDuration = 0.2f;
        [SerializeField] private float speedLineFadeOutDuration = 0.2f;

        private CanvasGroup _canvasGroup;
        [SerializeField] private Image _speedLineImage;
        private PlayerCameraSystem _playerCameraSystem;
        private PlayerRuntimeData _playerRuntimeData;
        private bool _isEffectVisible;

        [SerializeField] [ReadOnly] private bool _isInitialized;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _speedLineImage = GetComponent<Image>();
            _canvasGroup.alpha = 0f;
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
        

        private void OnDisable()
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.DOKill();
            _canvasGroup.alpha = 0f;
            _isEffectVisible = false;
        }
        
        public void ShowHUD()
        {
            if (speedLineSprites == null || speedLineSprites.Count == 0)
            {
                LogUtil.LogWarning("SpeedLineSprites 列表为空，无法显示速度线效果");
                return;
            }

            var index = Random.Range(0, speedLineSprites.Count);
            var selectedSprite = speedLineSprites[index];
            if (selectedSprite == null)
            {
                LogUtil.LogWarning($"SpeedLineSprites[{index}] 为 null，请检查配置");
                return;
            }

            _speedLineImage.sprite = selectedSprite;
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0.6f, speedLineFadeInDuration).SetUpdate(true);
            _isEffectVisible = true;
        }

        public void HideHUD()
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, speedLineFadeOutDuration).SetUpdate(true);
            _isEffectVisible = false;
        }

        public void UpdateHUDData(object data)
        {
        }

        public bool IsHUDVisible { get; }
        public bool IsHUDEnabled { get; set; }
        public bool IsDefaultHide { get; }=true;
    }
}