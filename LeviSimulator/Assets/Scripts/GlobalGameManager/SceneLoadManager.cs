using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GlobalGameManager
{
    public class SceneLoadManager : MonoBehaviour
    {
        public delegate void SceneLoadEventHandler(string sceneName);
        public delegate void SceneLoadProgressEventHandler(float progress);
    
        public event SceneLoadEventHandler OnSceneLoadStarted;
        public event SceneLoadEventHandler OnSceneLoadCompleted;
        public event SceneLoadProgressEventHandler OnSceneLoadProgress;

        public string loadingSceneName = "LoadingScene";
        private bool isLoading = false;
    
        public void LoadScene(SceneEnum sceneEnum, bool useLoadingScreen = true)
        {
            string sceneName = sceneEnum.GetSceneName();
            LoadSceneByName(sceneName, useLoadingScreen);
        }

        public void LoadSceneByName(string sceneName, bool useLoadingScreen = true)
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

        public void LoadSceneByIndex(int buildIndex, bool useLoadingScreen = true)
        {
            if (isLoading)
            {
                Debug.LogWarning("场景正在加载中，请等待...");
                return;
            }

            if (useLoadingScreen)
            {
                StartCoroutine(LoadSceneByIndexWithLoadingScreen(buildIndex));
            }
            else
            {
                SceneManager.LoadScene(buildIndex);
            }
        }

        private IEnumerator LoadSceneWithLoadingScreen(string targetSceneName)
        {
            isLoading = true;
            OnSceneLoadStarted?.Invoke(targetSceneName);

            // 先加载loading场景
            yield return SceneManager.LoadSceneAsync(loadingSceneName);

            // 异步加载目标场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnSceneLoadProgress?.Invoke(progress);

                if (asyncLoad.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(1f);
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            isLoading = false;
            OnSceneLoadCompleted?.Invoke(targetSceneName);
            
        }

        private IEnumerator LoadSceneByIndexWithLoadingScreen(int buildIndex)
        {
            isLoading = true;
            string targetSceneName = $"Scene_{buildIndex}";
            OnSceneLoadStarted?.Invoke(targetSceneName);

            // 先加载loading场景
            yield return SceneManager.LoadSceneAsync(loadingSceneName);

            // 异步加载目标场景
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(buildIndex);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnSceneLoadProgress?.Invoke(progress);

                if (asyncLoad.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(1f);
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            isLoading = false;
            OnSceneLoadCompleted?.Invoke(targetSceneName);
        }

        public void ReloadCurrentScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            LoadSceneByName(currentSceneName);
        }

        public bool IsLoading => isLoading;
    }
}