using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Utilities;

namespace GlobalGameManager
{
    /// <summary>
    /// 时间管理系统 - 处理暂停、子弹时间等
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [Title("时间控制设置")]
        [SerializeField] private bool canPause = true;
        [SerializeField] private bool canBulletTime = true;
        
        
        [Title("调试信息")]
        [SerializeField, ReadOnly] private float currentTimeScale = 1f;
        [SerializeField, ReadOnly] private bool isPaused = false;
        [SerializeField, ReadOnly] private bool isBulletTimeActive = false;
        private float _currentBulletTimeDuration = 0.3f;
        
        // 事件
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;
        public static event Action OnBulletTimeStarted;
        public static event Action OnBulletTimeEnded;
        public static event Action<float> OnTimeScaleChanged;
        
        // 属性
        public bool IsPaused => isPaused;
        public bool IsBulletTimeActive => isBulletTimeActive;
        public float CurrentTimeScale => currentTimeScale;
    
        public void OnEnable()
        {
            GlobalManager.Instance.gameStateManager.OnStateChanged+=HandleGameStateChanged;
        }

        
        /// <summary>
        ///  处理游戏状态变化
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void HandleGameStateChanged(GameState from, GameState to)
        {
            switch (to)
            {//todo 根据不同状态调整时间缩放
                case GameState.MainMenu:
                    ResumeGame();
                    break;
                case GameState.Loading:
                    ResumeGame();
                    break;
                case GameState.InGame:
                    ResumeGame();
                    break;
                case GameState.Paused:
                    PauseGame();
                    OnGamePaused?.Invoke();
                    break;
                case GameState.GameOver:
                    PauseGame();
                    break;
                case GameState.Settings:
                    break;
            }
        }

        public void Initialize()
        {
            ResetTimeManager();
        }
        

      
        
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        [Button("暂停游戏")]
        public void PauseGame()
        {
            if (!canPause || isPaused) return;
            
            isPaused = true;
            SetTimeScale(0f);
            OnGamePaused?.Invoke();
            
            LogUtil.Log("游戏已暂停");
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        [Button("恢复游戏")]
        public void ResumeGame()
        {
            if (!isPaused) return;
            
            isPaused = false;
            
            if (isBulletTimeActive)
                SetTimeScale(_currentBulletTimeDuration);
            else
                SetTimeScale(1f);
                
            OnGameResumed?.Invoke();
            LogUtil.Log("游戏已恢复");
        }
        /// <summary>
        /// 开始子弹时间
        /// </summary>
        public void StartBulletTime(float bulletTimeScale)
        {
            if (isPaused) return;

            _currentBulletTimeDuration = bulletTimeScale;
            isBulletTimeActive = true;
            SetTimeScale(_currentBulletTimeDuration);
            
            OnBulletTimeStarted?.Invoke();
            LogUtil.Log("子弹时间开始");
        }
        
        /// <summary>
        /// 结束子弹时间
        /// </summary>
        public void EndBulletTime()
        {
            if (!isBulletTimeActive) return;
            
            isBulletTimeActive = false;
            
            if (!isPaused)
                SetTimeScale(1f);
                
            OnBulletTimeEnded?.Invoke();
            LogUtil.Log("子弹时间结束");
        }
        
        /// <summary>
        /// 设置自定义时间缩放
        /// </summary>
        public void SetCustomTimeScale(float timeScale)
        {
            if (isPaused) return;
            
            SetTimeScale(timeScale);
        }
        
        private void SetTimeScale(float timeScale)
        {
            currentTimeScale = timeScale;
            Time.timeScale = timeScale;
            OnTimeScaleChanged?.Invoke(timeScale);
        }
        
        /// <summary>
        /// 重置时间管理器
        /// </summary>
        public void ResetTimeManager()
        {
            isPaused = false;
            isBulletTimeActive = false;
            SetTimeScale(1f);
        }


    }
}