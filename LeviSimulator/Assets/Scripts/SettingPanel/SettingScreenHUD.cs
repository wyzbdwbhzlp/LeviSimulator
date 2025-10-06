using Manager;
using UIManager;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace SettingPanel
{
    [ViewComponent("SettingsView")]
    public class SettingsView : MonoBehaviour, IViewComponent
    {
        [Header("Sliders")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private Slider sensitivitySlider;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        private SettingController _controller;
        private bool _uiBindingsInitialized;
        private bool _eventsSubscribed;
        private bool _isUIComponentActive;

        private void Awake()
        {
            ConfigureSliderRanges();
            InitializeUIBindings();
        }

        private void OnDisable()
        {
            UnsubscribeFromControllerEvents();
            _isUIComponentActive = false;
        }
        public void BindExitButtonEvent(UnityEngine.Events.UnityAction action)
        {
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(action);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromControllerEvents();
        }

        internal void AssignController(SettingController controller)
        {
            _controller = controller;
        }

        public void ShowUIPanel(object data)
        {
            EnsureController();
            SubscribeToControllerEvents();

            _isUIComponentActive = true;

            if (data is SettingsData settingsData)
            {
                ApplySettingsToUI(settingsData);
            }
            else if (_controller != null)
            {
                ApplySettingsToUI(_controller.CurrentSettings);
            }
            else
            {
                ApplySettingsToUI(SettingsData.Default);
            }

            _controller?.NotifyViewShown(this);
        }

        public void CloseUIPanel()
        {
            _isUIComponentActive = false;
            _controller?.NotifyViewClosed(this);
            UnsubscribeFromControllerEvents();
        }

        public bool IsUIComponentActive => _isUIComponentActive;

        #region Slider & Button Callbacks

        private void OnClickResume()
        {
            EnsureController()?.HandleResumeRequested();
        }

        private void OnClickRestart()
        {
            EnsureController()?.HandleRestartRequested();
        }

        private void OnClickExit()
        {
            EnsureController()?.HandleExitRequested();
        }

        private void UpdateBGMVolume(float value)
        {
            ApplyMixerVolume("BGMVolume", value);
            EnsureController()?.SetBgmVolume(value);
        }

        private void UpdateSFXVolume(float value)
        {
            ApplyMixerVolume("SFXVolume", value);
            EnsureController()?.SetSfxVolume(value);
        }

        private void UpdateFOV(float value)
        {
            var fovValue = ConvertFromSliderValue(fovSlider, value, SettingController.MinFieldOfView,
                SettingController.MaxFieldOfView);
            EnsureController()?.SetFieldOfView(fovValue);
            ApplyFieldOfView(fovValue);
        }

        private void UpdateSensitivity(float value)
        {
            var sensitivityValue = ConvertFromSliderValue(sensitivitySlider, value,
                SettingController.MinMouseSensitivity, SettingController.MaxMouseSensitivity);
            EnsureController()?.SetMouseSensitivity(sensitivityValue);
        }

        #endregion

        #region 内部工具方法

        private void InitializeUIBindings()
        {
            if (_uiBindingsInitialized)
            {
                return;
            }

            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.AddListener(UpdateBGMVolume);
            }
            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(UpdateSFXVolume);
            }
            if (fovSlider != null)
            {
                fovSlider.onValueChanged.AddListener(UpdateFOV);
            }
            if (sensitivitySlider != null)
            {
                sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
            }
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnClickResume);
            }
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnClickRestart);
            }
            if (exitButton != null)
            {
                exitButton.onClick.AddListener(OnClickExit);
            }

            _uiBindingsInitialized = true;
        }

        private SettingController EnsureController()
        {
            if (_controller == null)
            {
                _controller = SettingController.Instance;
            }

            return _controller;
        }

        private void SubscribeToControllerEvents()
        {
            if (_eventsSubscribed)
            {
                return;
            }

            var controller = EnsureController();
            if (controller == null)
            {
                return;
            }

            controller.SettingsDataChanged += HandleSettingsDataChanged;
            _eventsSubscribed = true;
        }

        private void UnsubscribeFromControllerEvents()
        {
            if (!_eventsSubscribed || _controller == null)
            {
                _eventsSubscribed = false;
                return;
            }

            _controller.SettingsDataChanged -= HandleSettingsDataChanged;
            _eventsSubscribed = false;
        }

        private void HandleSettingsDataChanged(SettingsData data)
        {
            ApplySettingsToUI(data);
        }

        private void ApplySettingsToUI(SettingsData data)
        {
            if (bgmSlider != null)
            {
                bgmSlider.SetValueWithoutNotify(data.BgmVolume);
                ApplyMixerVolume("BGMVolume", data.BgmVolume);
            }
            if (sfxSlider != null)
            {
                sfxSlider.SetValueWithoutNotify(data.SfxVolume);
                ApplyMixerVolume("SFXVolume", data.SfxVolume);
            }
            if (fovSlider != null)
            {
                var sliderValue = ConvertToSliderValue(fovSlider, data.FieldOfView,
                    SettingController.MinFieldOfView, SettingController.MaxFieldOfView);
                fovSlider.SetValueWithoutNotify(sliderValue);
                ApplyFieldOfView(data.FieldOfView);
            }
            if (sensitivitySlider != null)
            {
                var sliderValue = ConvertToSliderValue(sensitivitySlider, data.MouseSensitivity,
                    SettingController.MinMouseSensitivity, SettingController.MaxMouseSensitivity);
                sensitivitySlider.SetValueWithoutNotify(sliderValue);
            }
        }

        private void ApplyMixerVolume(string parameterName, float normalizedValue)
        {
            if (audioMixer == null)
            {
                return;
            }

            var clamped = Mathf.Clamp(normalizedValue, SettingController.MinVolumeNormalized, 1f);
            audioMixer.SetFloat(parameterName, Mathf.Log10(clamped) * 20f);
        }

        private static void ApplyFieldOfView(float value)
        {
            var mainCamera = Camera.main;
            if (mainCamera != null && !Mathf.Approximately(mainCamera.fieldOfView, value))
            {
                mainCamera.fieldOfView = value;
            }
        }

        private void ConfigureSliderRanges()
        {
            if (fovSlider != null && Mathf.Approximately(fovSlider.minValue, 0f) &&
                Mathf.Approximately(fovSlider.maxValue, 1f))
            {
                fovSlider.minValue = SettingController.MinFieldOfView;
                fovSlider.maxValue = SettingController.MaxFieldOfView;
            }

            if (sensitivitySlider != null && Mathf.Approximately(sensitivitySlider.minValue, 0f) &&
                Mathf.Approximately(sensitivitySlider.maxValue, 1f))
            {
                sensitivitySlider.minValue = SettingController.MinMouseSensitivity;
                sensitivitySlider.maxValue = SettingController.MaxMouseSensitivity;
            }
        }

        private static float ConvertFromSliderValue(Slider slider, float value, float defaultMin, float defaultMax)
        {
            if (slider == null)
            {
                return value;
            }

            var range = slider.maxValue - slider.minValue;
            if (Mathf.Approximately(range, 0f))
            {
                return value;
            }

            if (Mathf.Approximately(slider.minValue, 0f) && Mathf.Approximately(slider.maxValue, 1f))
            {
                return Mathf.Lerp(defaultMin, defaultMax, value);
            }

            return Mathf.Clamp(value, slider.minValue, slider.maxValue);
        }

        private static float ConvertToSliderValue(Slider slider, float actualValue, float defaultMin, float defaultMax)
        {
            if (slider == null)
            {
                return actualValue;
            }

            var range = slider.maxValue - slider.minValue;
            if (Mathf.Approximately(range, 0f))
            {
                return slider.minValue;
            }

            if (Mathf.Approximately(slider.minValue, 0f) && Mathf.Approximately(slider.maxValue, 1f))
            {
                return Mathf.InverseLerp(defaultMin, defaultMax, actualValue);
            }

            return Mathf.Clamp(actualValue, slider.minValue, slider.maxValue);
        }

        #endregion
    }
}
