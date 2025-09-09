using GlobalGameManager;
using UnityEngine;
using Utilities;

namespace SceneInteractionObject
{
    public class SceneExitTriggerItem:MonoBehaviour
    {
        [SerializeField]private SceneEnum nextScene=SceneEnum.SampleScene;
        private void Start()
        {
            if (nextScene == SceneEnum.SampleScene)
            {
                LogUtil.LogError("未设置下一个场景，请检查Inspector配置",true);
            }

            if (GetComponent<Collider>())
            {
                if (!GetComponent<Collider>().isTrigger)
                {
                    LogUtil.LogWarning("关卡退出点的Collider未设置为Trigger，请检查配置",true);
                }
            }
            else
            {
                LogUtil.LogError("关卡退出点未设置Collider组件，请检查配置",true);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                LogUtil.Log("触发场景切换到"+nextScene,true);
                GlobalManager.Instance.playerSpawnManager.DespawnAllPlayers();
                GlobalManager.Instance.sceneLoadManager.LoadScene(nextScene);
            }
        }
        
    }
}