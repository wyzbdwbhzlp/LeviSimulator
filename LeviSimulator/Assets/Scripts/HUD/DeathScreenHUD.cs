
using DG.Tweening;
using UnityEngine;
using Utilities;
using GlobalGameManager;
using Sirenix.OdinInspector;
using UIManager; // 新增

namespace HUD
{
    [HUD("DeathScreenHUD")]
    public class DeathScreenHUD:MonoBehaviour
    {
        [SerializeField]private CanvasGroup blackOutCanvas;
        private Coroutine _fadeCoroutine;

        [Header("黑屏参数")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.35f;

        [Header("复活输入")]
        [SerializeField] private KeyCode respawnKey = KeyCode.R;
        [SerializeField] private bool allowAnyKeyToRespawn = false;

        private Tween _fadeTween;
        private Coroutine _waitRespawnCoroutine;

        private void Awake()
        {
            blackOutCanvas = GetComponent<CanvasGroup>();
            if (blackOutCanvas == null)
            {
                LogUtil.LogError("DeathScreenHUD: CanvasGroup 组件未设置，请检查Inspector配置", true);
            }
            else
            {
                blackOutCanvas.alpha = 0; // 初始时隐藏黑屏
                blackOutCanvas.interactable = false;
                blackOutCanvas.blocksRaycasts = false;
            }
        }

        private void OnEnable()
        {
            // 订阅游戏状态变更，联动GameOver与InGame
            var gm = GlobalManager.Instance;
            if (gm != null && gm.gameStateManager != null)
            {
                gm.gameStateManager.OnStateChanged += OnGameStateChanged;
            }
            else
            {
                LogUtil.LogError("找不到 GameStateManager，无法监听游戏状态变化", true);
            }
        }

        private void OnDisable()
        {
            var gm = GlobalManager.Instance;
            if (gm != null && gm.gameStateManager != null)
            {
                gm.gameStateManager.OnStateChanged -= OnGameStateChanged;
            }
            CleanUp();
        }
        

        [Button ("测试黑屏效果")]
        private void OnGameStateChanged(GameState from, GameState to)
        {
            if (to == GameState.GameOver)
            {
                ShowBlackout();
                StartListeningToRespawn();
            }
            else if (from == GameState.GameOver && to == GameState.InGame)
            {
                HideBlackout();
            }
        }

        /// <summary>
        ///  切换到黑屏状态
        /// </summary>
        private void ShowBlackout()
        {
            // 先停止任何淡出/监听
            StopRespawnListening();
            _fadeTween?.Kill();

            if (blackOutCanvas == null)
            {
                LogUtil.LogError("DeathScreenHUD: 缺少 CanvasGroup，无法显示黑屏", true);
                return;
            }

            blackOutCanvas.interactable = true;
            blackOutCanvas.blocksRaycasts = true;
            
            _fadeTween = blackOutCanvas
                .DOFade(1f, fadeInDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    //todo 冻结玩家移动输入
                });
        }

        /// <summary>
        ///  开始监听玩家复活按钮触发
        /// </summary>
        private void StartListeningToRespawn()
        {
            StopRespawnListening();
            _waitRespawnCoroutine = StartCoroutine(WaitForRespawnInput());
        }

        private System.Collections.IEnumerator WaitForRespawnInput()
        {
            while (true)
            {
                if (allowAnyKeyToRespawn ? Input.anyKeyDown : Input.GetKeyDown(respawnKey))
                {
                    // 复活玩家
                    var gm = GlobalManager.Instance;
                    if (gm != null && gm.playerSpawnManager != null)
                    {
                        gm.playerSpawnManager.RebirthPlayer();
                    }
                    else
                    {
                        LogUtil.LogError("DeathScreenHUD: 找不到 PlayerSpawnManager，无法复活玩家", true);
                    }

                    // 切回游戏
                    gm?.gameStateManager?.ChangeState(GameState.InGame);
                    yield break;
                }
                yield return null;
            }
        }

        private void StopRespawnListening()
        {
            if (_waitRespawnCoroutine != null)
            {
                StopCoroutine(_waitRespawnCoroutine);
                _waitRespawnCoroutine = null;
            }
        }

        private void HideBlackout()
        {
            StopRespawnListening();
            _fadeTween?.Kill();

            if (blackOutCanvas == null) return;

            _fadeTween = blackOutCanvas
                .DOFade(0f, fadeOutDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    blackOutCanvas.interactable = false;
                    blackOutCanvas.blocksRaycasts = false;
                    //todo  解冻玩家移动
                });
        }

        private void CleanUp()
        {
            StopRespawnListening();
            _fadeTween?.Kill();
            _fadeTween = null;
        }
        private void OnDestroy()
        {
            CleanUp();
        }
    }
}