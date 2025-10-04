using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Slider fovSlider;
    public Slider sensitivitySlider;

    [Header("Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button exitButton;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    private void Start()
    {
        // 初始化 Slider 值 
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        fovSlider.value = PlayerPrefs.GetFloat("FOV", 60f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);

        // 绑定监听
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        fovSlider.onValueChanged.AddListener(SetFOV);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);

        resumeButton.onClick.AddListener(OnResume);
        restartButton.onClick.AddListener(OnRestart);
        exitButton.onClick.AddListener(OnQuit);
    }

    // ---- 音量 ----
    public void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // ---- 相机视角 ----
    public void SetFOV(float value)
    {
        // TODO
        PlayerPrefs.SetFloat("FOV", value);
    }

    // ---- 鼠标灵敏度 ----
    public void SetSensitivity(float value)
    {
        // TODO
        PlayerPrefs.SetFloat("MouseSensitivity", value);
    }

    // ---- UI 按钮 ----
    public void OnResume()
    {
        // TODO: 继续游戏
    }

    public void OnRestart()
    {
        // TODO: 重新开始
    }

    public void OnQuit()
    {
        // TODO: 退出游戏
    }
}
