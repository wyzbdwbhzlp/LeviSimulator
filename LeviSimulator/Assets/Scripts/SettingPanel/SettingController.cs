using System;
using GlobalGameManager;
using PlayerControllers.Refactored;
using Sirenix.OdinInspector;
using UIManager;
using UnityEngine;

namespace SettingPanel
{
    /// <summary>
    /// 常量化的 PlayerPrefs key
    /// </summary>
    public static class SettingPreferenceKeys
    {
        public const string BgmVolume = "BGMVolume";
        public const string SfxVolume = "SFXVolume";
        public const string FieldOfView = "FOV";
        public const string MouseSensitivity = "MouseSensitivity";
    }

    [Serializable]
    public struct SettingsData
    {
        public float BgmVolume;
        public float SfxVolume;
        public float FieldOfView;
        public float MouseSensitivity;

        public static SettingsData Default => new SettingsData
        {
            BgmVolume = SettingController.DefaultBgmVolume,
            SfxVolume = SettingController.DefaultSfxVolume,
            FieldOfView = SettingController.DefaultFieldOfView,
            MouseSensitivity = SettingController.DefaultMouseSensitivity
        };

        public static SettingsData LoadFromPrefs()
        {
            var data = Default;
            data.BgmVolume = Mathf.Clamp(PlayerPrefs.GetFloat(SettingPreferenceKeys.BgmVolume, SettingController.DefaultBgmVolume), SettingController.MinVolumeNormalized, 1f);
            data.SfxVolume = Mathf.Clamp(PlayerPrefs.GetFloat(SettingPreferenceKeys.SfxVolume, SettingController.DefaultSfxVolume), SettingController.MinVolumeNormalized, 1f);
            data.FieldOfView = Mathf.Clamp(PlayerPrefs.GetFloat(SettingPreferenceKeys.FieldOfView, SettingController.DefaultFieldOfView), SettingController.MinFieldOfView, SettingController.MaxFieldOfView);
            data.MouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(SettingPreferenceKeys.MouseSensitivity, SettingController.DefaultMouseSensitivity), SettingController.MinMouseSensitivity, SettingController.MaxMouseSensitivity);
            return data;
        }
    }

    /// <summary>
    /// 负责管理设置数据、UI 面板生命周期及对外事件的控制器。
    /// </summary>
    public class SettingController : MonoBehaviour
    {
        #region 常量配置

        public const float DefaultBgmVolume = 0.75f;
        public const float DefaultSfxVolume = 0.75f;
        public const float DefaultFieldOfView = 60f;
        public const float DefaultMouseSensitivity = 1f;

        public const float MinVolumeNormalized = 0.0001f;
        public const float MinFieldOfView = 40f;
        public const float MaxFieldOfView = 120f;
        public const float MinMouseSensitivity = 0.05f;
        public const float MaxMouseSensitivity = 10f;

        #endregion

        #region 单例管理

        private static SettingController _instance;

        /// <summary>
        /// 获取（或自动创建）设置控制器实例。
        /// </summary>
        public static SettingController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SettingController>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[SettingController]");
                        _instance = go.AddComponent<SettingController>();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// 是否已经存在实例。
        /// </summary>
        public static bool HasInstance => _instance != null;

        #endregion

        #region 运行时状态

        private SettingsData _settingsData = SettingsData.Default;
        private SettingsView _activeView;
        private bool _settingsLoaded;

        #endregion

        #region 对外事件
        
        public event Action<SettingsData> SettingsDataChanged;
        public event Action<float> BgmVolumeChanged;
        public event Action<float> SfxVolumeChanged;
        public event Action<float> FieldOfViewChanged;
        public event Action<float> MouseSensitivityChanged;
        
        public event Action<bool> PanelVisibilityChanged;
        
        public event Action ResumeRequested;
        public event Action RestartRequested;
        public event Action ExitRequested;

        #endregion

        #region 生命周期

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (!_settingsLoaded)
            {
                _settingsData = SettingsData.LoadFromPrefs();
                _settingsLoaded = true;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            FlushToDisk();
        }

        #endregion

        #region 属性与基本操作

        /// <summary>当前缓存的设置数据。</summary>
        public SettingsData CurrentSettings => _settingsData;

        /// <summary>当前设置面板是否可见。</summary>
        public bool IsSettingsVisible => _activeView != null && _activeView.IsUIComponentActive;

        /// <summary>手动保存 PlayerPrefs。</summary>
        public void FlushToDisk()
        {
            PlayerPrefs.SetFloat(SettingPreferenceKeys.BgmVolume, _settingsData.BgmVolume);
            PlayerPrefs.SetFloat(SettingPreferenceKeys.SfxVolume, _settingsData.SfxVolume);
            PlayerPrefs.SetFloat(SettingPreferenceKeys.FieldOfView, _settingsData.FieldOfView);
            PlayerPrefs.SetFloat(SettingPreferenceKeys.MouseSensitivity, _settingsData.MouseSensitivity);
            PlayerPrefs.Save();
        }
        

        #endregion

        #region 设置修改接口

        public bool SetBgmVolume(float normalizedVolume, bool save = true, bool notify = true)
        {
            normalizedVolume = Mathf.Clamp(normalizedVolume, MinVolumeNormalized, 1f);
            if (Mathf.Approximately(_settingsData.BgmVolume, normalizedVolume))
                return false;

            _settingsData.BgmVolume = normalizedVolume;
            if (save)
            {
                PlayerPrefs.SetFloat(SettingPreferenceKeys.BgmVolume, normalizedVolume);
            }

            if (notify)
            {
                BgmVolumeChanged?.Invoke(normalizedVolume);
                SettingsDataChanged?.Invoke(_settingsData);
            }

            return true;
        }

        public bool SetSfxVolume(float normalizedVolume, bool save = true, bool notify = true)
        {
            normalizedVolume = Mathf.Clamp(normalizedVolume, MinVolumeNormalized, 1f);
            if (Mathf.Approximately(_settingsData.SfxVolume, normalizedVolume))
                return false;

            _settingsData.SfxVolume = normalizedVolume;
            if (save)
            {
                PlayerPrefs.SetFloat(SettingPreferenceKeys.SfxVolume, normalizedVolume);
            }

            if (notify)
            {
                SfxVolumeChanged?.Invoke(normalizedVolume);
                SettingsDataChanged?.Invoke(_settingsData);
            }

            return true;
        }

        public bool SetFieldOfView(float fov, bool save = true, bool notify = true)
        {
            fov = Mathf.Clamp(fov, MinFieldOfView, MaxFieldOfView);
            if (Mathf.Approximately(_settingsData.FieldOfView, fov))
                return false;

            _settingsData.FieldOfView = fov;
            if (save)
            {
                PlayerPrefs.SetFloat(SettingPreferenceKeys.FieldOfView, fov);
            }

            if (notify)
            {
                FieldOfViewChanged?.Invoke(fov);
                SettingsDataChanged?.Invoke(_settingsData);
            }

            return true;
        }

        public bool SetMouseSensitivity(float sensitivity, bool save = true, bool notify = true)
        {
            sensitivity = Mathf.Clamp(sensitivity, MinMouseSensitivity, MaxMouseSensitivity);
            if (Mathf.Approximately(_settingsData.MouseSensitivity, sensitivity))
                return false;

            _settingsData.MouseSensitivity = sensitivity;
            if (save)
            {
                PlayerPrefs.SetFloat(SettingPreferenceKeys.MouseSensitivity, sensitivity);
            }

            if (notify)
            {
                MouseSensitivityChanged?.Invoke(sensitivity);
                SettingsDataChanged?.Invoke(_settingsData);
            }

            return true;
        }

        #endregion

        #region 面板生命周期

        [Button("显示设置面板")]
        public void ShowSettings()
        {
            if (IsSettingsVisible)
            {
                return;
            }

            var view = MainUIManager.ShowUIComponent<SettingsView>();
            if (view == null)
            {
                Debug.LogError("SettingsView 未能成功实例化，请检查预制体或特性配置。");
                return;
            }
            GlobalManager.Instance.gameStateManager.PauseGame();
            _activeView = view;
            view.AssignController(this);
            view.ShowUIPanel(_settingsData);
            view.BindExitButtonEvent(HideSettings);
            PlayerController.UnlockAndShowCursor();
        }

        [Button("隐藏设置面板")]
        public void HideSettings()
        {
            if (!HasView())
            {
                return;
            }
            GlobalManager.Instance.gameStateManager.ResumeGame();
            MainUIManager.HideUIComponent<SettingsView>();
            PlayerController.LockAndHideCursor();
        }

        public void ToggleSettings()
        {
            if (IsSettingsVisible)
            {
                HideSettings();
            }
            else
            {
                ShowSettings();
            }
        }

        internal void NotifyViewShown(SettingsView view)
        {
            _activeView = view;
            PanelVisibilityChanged?.Invoke(true);
        }

        internal void NotifyViewClosed(SettingsView view)
        {
            if (_activeView == view)
            {
                _activeView = null;
                PanelVisibilityChanged?.Invoke(false);
            }
        }

        private bool HasView()
        {
            return _activeView != null;
        }

        #endregion

        #region 面板按钮事件转发

        internal void HandleResumeRequested()
        {
            ResumeRequested?.Invoke();
            HideSettings();
            FlushToDisk();
            PlayerController.LockAndHideCursor();
        }

        internal void HandleRestartRequested()
        {
            RestartRequested?.Invoke();
        }

        internal void HandleExitRequested()
        {
            FlushToDisk();
            ExitRequested?.Invoke();
        }

        #endregion
    }
}