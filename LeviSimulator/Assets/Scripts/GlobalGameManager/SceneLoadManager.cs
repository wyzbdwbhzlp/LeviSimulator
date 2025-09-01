using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlobalGameManager
{
    public class SceneLoadManager : MonoBehaviour
    {
    
        [Header("场景配置")]
        public string mainMenuSceneName = "MainMenu";
        public string gameSceneName = "GameScene";
        public string loadingSceneName = "LoadingScene";

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        public event Action<float> OnSceneLoadProgress;

        private bool isLoading = false;

        public void Initialize()
        {
            Debug.Log("SceneLoadManager 初始化完成");
        }

        public void LoadScene(string sceneName, bool useLoadingScreen = true)
        {
            if (isLoading)
            {
                Debug.LogWarning("场景正在加载中，请等待...");
                return;
            }

            if (useLoadingScreen)
            {
                StartCoroutine(LoadSceneWithLoadingScreen(sceneName));
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private IEnumerator LoadSceneWithLoadingScreen(string targetScene)
        {
            isLoading = true;
            OnSceneLoadStarted?.Invoke(targetScene);

            // 先加载loading场景
            yield return SceneManager.LoadSceneAsync(loadingSceneName);

            // 异步加载目标场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnSceneLoadProgress?.Invoke(progress);

                if (asyncLoad.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(1f); // 最少显示1秒loading
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            isLoading = false;
            OnSceneLoadCompleted?.Invoke(targetScene);
        }

        public void LoadMainMenu()
        {
            LoadScene(mainMenuSceneName);
        }

        public void LoadGameScene()
        {
            LoadScene(gameSceneName);
        }

        public void ReloadCurrentScene()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene);
        }
    }
}
