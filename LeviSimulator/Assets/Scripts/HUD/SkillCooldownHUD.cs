using Sirenix.OdinInspector;
using UIManager;
using UnityEngine;
using UnityEngine.UI;

namespace HUD
{
    [HUD("SkillCooldownHUD")]
    public class SkillCooldownHUD : MonoBehaviour, IHUDComponent
    {
        private const float DashCooldownDuration = 5f;

        [SerializeField] private Transform skillIconContainer;
        [SerializeField] [LabelText("冲刺冷却遮罩")] private Image dashCooldownMask;

        [ShowInInspector] [ReadOnly] private float _remainingCooldown;
        [ShowInInspector] [ReadOnly] private bool _isCoolingDown;

        private GameStateManager _gameStateManager; //tip 游戏状态管理器引用
        private void Awake()
        {
            if (skillIconContainer == null)
            {
                Debug.LogError("SkillCooldownHUD: skillIconContainer 未设置，请检查Inspector配置", this);
            }
            if (dashCooldownMask == null)
            {
                Debug.LogError("SkillCooldownHUD: dashCooldownMask 未设置，请检查Inspector配置", this);
            }

            RegisterMe();
        }
        private void SubribeEvents()
        {
            var gameStateManager = GlobalGameManager.GlobalManager.Instance.gameStateManager;
            if (gameStateManager != null)
            {
                gameStateManager.OnStateChanged+= HandleGameStateChanged;
            }

        }
        private void OnEnable()
        {
            SubribeEvents();
            ResetCooldown();
        }
        private void OnDisable()
        {
            UnsubribeEvents();
        }

        private void HandleGameStateChanged(GameState arg1, GameState arg2)
        {
            if (arg2 != GameState.InGame)
            {
                HideHUD();
            }
            else
            {
                ShowHUD();
            }
        }

        private void UnsubribeEvents()
        {
            if (_gameStateManager != null)
            {
                _gameStateManager.OnStateChanged -= HandleGameStateChanged;
            }
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
            skillIconContainer.gameObject.SetActive(true);
        }

        public void HideHUD()
        {
            skillIconContainer.gameObject.SetActive(false);
        }

        public void UpdateHUDData(object data)
        {
        }
        

        private void Update()
        {
            if (!_isCoolingDown)
            {
                return;
            }

            if (_remainingCooldown > 0f)
            {
                _remainingCooldown -= Time.deltaTime;
                if (_remainingCooldown <= 0f)
                {
                    _remainingCooldown = 0f;
                    _isCoolingDown = false;
                }
                ApplyFillAmount(_remainingCooldown / DashCooldownDuration);
            }
        }

        public void BeginCooldown()
        {
            _remainingCooldown = DashCooldownDuration;
            _isCoolingDown = true;
            ApplyFillAmount(1f);
        }

        private void ResetCooldown()
        {
            _remainingCooldown = 0f;
            _isCoolingDown = false;
            ApplyFillAmount(0f);
        }

        private void ApplyFillAmount(float percent)
        {
            if (dashCooldownMask != null)
            {
                dashCooldownMask.fillAmount = Mathf.Clamp01(percent);
            }
        }

        public bool IsHUDVisible => skillIconContainer != null && skillIconContainer.gameObject.activeSelf;
        public bool IsHUDEnabled { get; set; }
        public bool IsDefaultHide => false;
    }
}