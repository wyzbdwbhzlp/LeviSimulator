using System;
using GlobalGameManager;
using PlayerControllers.Refactored;
using UIManager;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Sirenix.OdinInspector;

namespace HUD
{
    [HUD("CrosshairHUD")]
    public class CrosshairHUD:MonoBehaviour, IHUDComponent
    {
        [SerializeField]private Image crosshairImage;
        private RectTransform _crosshairRectTransform;
        private CanvasGroup _canvasGroup;
        
        private Camera _mainCamera;
        private GameStateManager _gameStateManager;

        private void Awake()
        {
            _canvasGroup= GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            RegisterMe();
        }

        public void OnEnable()
        {
            // // 初始化摄像机引用
            // if (_mainCamera == null)
            // {
            //     _mainCamera = GetMainCamera();
            //     if (_mainCamera != null)
            //     {
            //         LogUtil.LogWarning("未成功获取摄像头", true);
            //     }
            // }
            
            // 初始化 RectTransform 引用
            if (_crosshairRectTransform == null && crosshairImage != null)
            {
                _crosshairRectTransform = crosshairImage.GetComponent<RectTransform>();
            }
            // 校准准星位置
            if(_mainCamera != null) 
                CalibratePosition();

            RefreashEvents();
        }
        private void OnDisable()
        {
            UnsubribeFromEvents();
        }
        private void RefreashEvents()
        {
            UnsubribeFromEvents();
            SubribeToEvents();
        }
        private void SubribeToEvents()
        {
            var gameStateManager = GlobalManager.Instance.gameStateManager;
            if (gameStateManager != null)  // 修复:当不为 null 时才订阅事件
            {
                _gameStateManager = gameStateManager;
                _gameStateManager.OnStateChanged += OnGameStateChanged;
            }
            else
            {
                LogUtil.LogError("CrosshairHUD: 无法获取 GameStateManager,事件订阅失败", true);
            }
        }

        private void OnGameStateChanged(GameState arg1, GameState arg2)
        {
            if (arg2 == GameState.InGame)
            {
                ShowHUD();
                return;
            }
            HideHUD();
            
        }

        private void UnsubribeFromEvents()
        {
            if (_gameStateManager!=null)
            {
                _gameStateManager.OnStateChanged -= OnGameStateChanged;
            }

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

        [Button("ShowHUD")]
        public void ShowHUD()
        {
            _canvasGroup.alpha = 1;
        }
        
        [Button("HideHUD")]
        public void HideHUD()
        {
            _canvasGroup.alpha = 0;
        }

        public void UpdateHUDData(object data)
        {
            LogUtil.Log("CrosshairHUD UpdateHUDData");
        }
        /// <summary>
        /// 校准准星位置,使其总是在屏幕正中间
        /// 射击着陆点以 _cameraTransform.forward 为准
        /// </summary>
        private void CalibratePosition()
        {
            if (_crosshairRectTransform == null)
            {
                LogUtil.LogWarning("CrosshairHUD: RectTransform 未设置,无法校准位置");
                return;
            }
            
            _crosshairRectTransform.anchoredPosition = Vector2.zero;
            
            _crosshairRectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            _crosshairRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            _crosshairRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            
        }
        
        /// <summary>
        /// 获取主摄像机引用
        /// 优先从 PlayerController 获取,否则使用 Camera.main
        /// </summary>
        /// <returns>摄像机引用</returns>
        private Camera GetMainCamera()
        {
            var playerController = GlobalManager.Instance.playerSpawnManager.GetCurrentPlayer().GetComponent<PlayerController>();
            return playerController.CameraSystem.Camera;
        }

        public bool IsHUDVisible { get; }
        public bool IsHUDEnabled { get; set; }
        public bool IsDefaultHide { get; } = false;
    }
}