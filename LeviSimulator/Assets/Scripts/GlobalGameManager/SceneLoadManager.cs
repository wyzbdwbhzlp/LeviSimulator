using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace GlobalGameManager
{
    public class SceneLoadManager : MonoBehaviour
    {
        public delegate void SceneLoadEventHandler(SceneEnum sceneEnum);
        public delegate void SceneLoadProgressEventHandler(float progress);
    
        public event SceneLoadEventHandler OnSceneLoadStarted;
        public event SceneLoadEventHandler OnSceneLoadCompleted;
        public event SceneLoadProgressEventHandler OnSceneLoadProgress;

        private const SceneEnum LoadingSceneEnum = SceneEnum.LoadingScene; // Loading场景
        private const SceneEnum HUDScene= SceneEnum.HUDScene; // HUD场景
        private bool isLoading = false;

        private static readonly SceneEnum[] TheLevelScenes = new SceneEnum[]// 游戏关卡场景
        {
            SceneEnum.Level1,
            SceneEnum.Level2zhuizhu,
            SceneEnum.Level3bianhuan,
            SceneEnum.Level4pohuai,
            SceneEnum.Level5end,
            SceneEnum.Home
        };


        public void LoadScene(SceneEnum sceneEnum, bool useLoadingScreen = true)
        {
            LoadSceneByName(sceneEnum, useLoadingScreen);
        }

        public void LoadSceneByName(SceneEnum sceneEnum, bool useLoadingScreen = true)
        {
            if (isLoading)
            {
                Debug.LogWarning("场景正在加载中，请等待...");
                return;
            }

            if (useLoadingScreen)
            {
                StartCoroutine(LoadSceneWithLoadingScreen(sceneEnum));
            }
            else
            {
                SceneManager.LoadScene(sceneEnum.GetSceneName());
            }
        }
        

        private IEnumerator LoadSceneWithLoadingScreen(SceneEnum sceneEnum)
        {
            var targetSceneName = sceneEnum.GetSceneName();
            isLoading = true;
            OnSceneLoadStarted?.Invoke(sceneEnum);

            // 先加载loading场景
            var loadingScene = SceneManager.GetSceneByName(LoadingSceneEnum.GetSceneName());
            if (!loadingScene.IsValid() || !loadingScene.isLoaded)
            {
                yield return SceneManager.LoadSceneAsync(LoadingSceneEnum.GetSceneName());
            }
            else
            {
                LogUtil.LogWarning("Loading场景已经加载或者不存在，请注意检查");
            }

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

            if (TheLevelScenes.Contains(sceneEnum))
            {
                // 确保HUD场景被加载
                var hudScene = SceneManager.GetSceneByName(HUDScene.GetSceneName());
                if (!hudScene.IsValid() || !hudScene.isLoaded)
                {
                    yield return SceneManager.LoadSceneAsync(HUDScene.GetSceneName(), LoadSceneMode.Additive);
                }
            }else
            {
                var hudScene = SceneManager.GetSceneByName(HUDScene.GetSceneName());
                if (hudScene.IsValid() && hudScene.isLoaded)
                {
                    yield return SceneManager.UnloadSceneAsync(hudScene);
                }
            }
            //yield return SceneManager.UnloadSceneAsync(LoadingSceneEnum.GetSceneName());

            isLoading = false;
            OnSceneLoadCompleted?.Invoke(sceneEnum);
            
        }

        public void ReloadCurrentScene()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            TryGetSceneEnum(currentSceneName, out var sceneEnum);
            LoadSceneByName(sceneEnum);
        }

        public bool IsLoading => isLoading;

        private bool TryGetSceneEnum(string sceneName, out SceneEnum sceneEnum)
        {
            bool effective =Enum.TryParse(sceneName, out sceneEnum);
            if (!effective)
            {
                LogUtil.LogError($"无法将场景名 {sceneName} 转换为 SceneEnum 枚举，请检查枚举定义是否包含该场景名", true);
                return false;
            }
            return true;
        }
    }
}