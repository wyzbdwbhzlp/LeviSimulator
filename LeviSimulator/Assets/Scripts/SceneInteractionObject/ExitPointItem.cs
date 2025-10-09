using System;
using ExternPropertyAttributes;
using GlobalGameManager;
using UnityEngine;
using Utilities;

namespace SceneInteractionObject
{
    public class ExitPointItem:MonoBehaviour
    {
        [SerializeField][Label("目标场景")] private SceneEnum nextScene=SceneEnum.SampleScene;
        [SerializeField][Label("是否记录为通关")]private bool markAsCompleted = true;
        [SerializeField][Label("是否需要前置关卡通关")]private bool requirePreviousLevelCompleted = false;
        [SerializeField][Label("前置关卡")] [ShowIf("requirePreviousLevel")] private SceneEnum previousLevel = SceneEnum.Level1;
        [SerializeField][Label("传送门实体")][ShowIf("requirePreviousLevel")]private GameObject doorObject;
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

        private void OnEnable()
        {
            if (requirePreviousLevelCompleted && doorObject != null)
            {
                var gsm = GlobalManager.Instance;
                if (gsm != null)
                {
                    var levelCompletionStatus = gsm.LevelCompletionStatus;
                    if (levelCompletionStatus.TryGetValue(previousLevel, out var value))
                    {
                        if (value)
                        {
                            doorObject.SetActive(true); // 前置关卡已完成，显示传送门
                        }
                        else
                        {
                            doorObject.SetActive(false);
                        }
                    }
                    else
                    {
                        doorObject.SetActive(true); // 未找到前置关卡状态，默认显示传送门
                    }
                }
                else
                {
                    LogUtil.LogError("无法获取 GameStateManager 实例。");
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (VerifyPreviousLevelCompleted())
                {
                    LogUtil.Log($"玩家进入了出口点，准备切换到场景: {nextScene}");
                    if (markAsCompleted)
                    {
                        CompletedLevel();
                    }

                    GlobalManager.Instance.playerSpawnManager.DespawnAllPlayers(); // 先卸载所有玩家
                    GlobalManager.Instance.sceneLoadManager.LoadScene(nextScene); // 加载新场景
                }
            }
        }
        private void CompletedLevel()
        {
            var gsm = GlobalManager.Instance;
            if (gsm != null)
            {
                gsm.CompetedLevel();
            }
            else
            {
                LogUtil.LogError("无法标记关卡为已完成，GameStateManager 未找到。");
            }
            
        }

        private bool VerifyPreviousLevelCompleted()
        {
            if(requirePreviousLevelCompleted==false)return true;
            var gsm = GlobalManager.Instance;
            if (gsm != null)
            {
                var levelCompletionStatus = gsm.LevelCompletionStatus;
                if (levelCompletionStatus.TryGetValue(previousLevel, out var value))
                {
                    return value; // 返回前置关卡的完成状态
                }
                else
                {
                    return true; 
                }

            }

            return true;
        }
    }
}