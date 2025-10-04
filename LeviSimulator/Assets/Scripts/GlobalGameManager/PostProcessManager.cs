using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PostProcessManager : MonoBehaviour
{
    [Header("统一使用的Volume Profile")]
    public VolumeProfile globalProfile;

    private Volume globalVolume;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupGlobalVolume();
    }

    private void SetupGlobalVolume()
    {
        if (globalProfile == null) return;

        // 查找当前场景中是否已经有 Global Volume
        globalVolume = FindObjectOfType<Volume>();
        if (globalVolume == null || globalVolume.isGlobal == false)
        {
            GameObject volumeObj = new GameObject("GlobalVolume");
            globalVolume = volumeObj.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.priority = 100f;
            globalVolume.profile = globalProfile;
        }
    }
}
