using ExternPropertyAttributes;
using GlobalGameManager;
using UnityEngine;
using Utilities;

namespace SceneInteractionObject
{
    public class ExitPointItem:MonoBehaviour
    {
        [SerializeField][Label("目标场景")] private SceneEnum nextScene=SceneEnum.SampleScene;
        private void Awake()
        {
            var exitPointcollider = GetComponent<Collider>();
            if (exitPointcollider == null)
            {
                LogUtil.LogError("CheckPointItem 需要一个 Collider 组件来检测玩家的进入和离开。");
            }
            else
            {
                exitPointcollider.isTrigger = true; //tip 确保 Collider 设置为 Trigger
            }


            if (nextScene == SceneEnum.SampleScene)
            {
                LogUtil.LogWarning("ExitPointItem 的 nextScene 未设置，默认值为 SampleScene。请在 Inspector 中设置正确的目标场景。");
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                LogUtil.Log($"玩家进入了出口点，准备切换到场景: {nextScene}");
                GlobalManager.Instance.playerSpawnManager.DespawnAllPlayers();// 先卸载所有玩家
                GlobalManager.Instance.sceneLoadManager.LoadScene(nextScene); // 加载新场景
            }
        }
    }
}