using GlobalGameManager;
using UnityEngine;
using Utilities;

namespace MainMenuView
{
    public class MainMenuView:MonoBehaviour
    {
        public void OnStartButtonClicked()
        {
            LogUtil.Log("Start Game button clicked!");
            var gm = GlobalManager.Instance;
            if (gm != null)
            {
                var levelCompletionStatus = gm.LevelCompletionStatus;
                if (levelCompletionStatus.TryGetValue(SceneEnum.Level1, out var value))
                {
                    if (value == false)
                    {
                        gm.sceneLoadManager.LoadScene(SceneEnum.Level1);
                    }
                    else
                    {
                        gm.sceneLoadManager.LoadScene(SceneEnum.Home);
                    }
                }
            }
        }
        
        public void OnExitButtonClicked()
        {
            // 退出游戏的逻辑
            LogUtil.Log("Exit button clicked!");
            Application.Quit();
        }
        
    }
}