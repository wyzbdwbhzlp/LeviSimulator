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
                gm.sceneLoadManager.LoadScene(SceneEnum.Level1);
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