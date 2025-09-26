using System;
using Game.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace SceneInteractionObject
{
    public enum BgmDirectorTriggerTypeEnum
    {
        TriggerEnter, //进入触发
        EchoInteraction //回声交互触发
    }
    [RequireComponent(typeof(Collider))]
    public class BgmDirectorPointItem:MonoBehaviour
    {
        [LabelText("触发方式")]
        [SerializeField]private BgmDirectorTriggerTypeEnum triggerType=BgmDirectorTriggerTypeEnum.TriggerEnter;
        
        [LabelText("触发动作")]
        [SerializeField]private BgmDirectorTriggerActionEnum triggerAction=BgmDirectorTriggerActionEnum.NoAction;

        [LabelText("附加信息")] [SerializeField]
        private ActionAdditionalInfoEnum additionalInfo = ActionAdditionalInfoEnum.NoInfo;
        [LabelText("BGM开始时间(秒)")]
        [ShowIf("BgmStartTimeSecondsShow")][SerializeField]private float bgmStartTimeSeconds;
        
        private EchoInteractable _echoInteractable;
        private bool _isUsed=false;
        private void Awake()
        {
            var _collider = GetComponent<Collider>();
            _collider.isTrigger = true;

            _echoInteractable = GetComponent<EchoInteractable>();
        }

        
        private void OnEnable()
        {
            if(_echoInteractable!=null)
                _echoInteractable.OnEchoInteracted+=OnHandleEchoInteraction;
        }
        private void OnDisable()
        {
            if(_echoInteractable!=null)
                _echoInteractable.OnEchoInteracted-=OnHandleEchoInteraction;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(triggerType!=BgmDirectorTriggerTypeEnum.TriggerEnter) return;
            if(triggerAction== BgmDirectorTriggerActionEnum.NoAction) return;
            if (other.CompareTag("Player"))
            {
                ExecuteTriggerAction();
            }
        }

        private void ExecuteTriggerAction()
        {
            if (additionalInfo == ActionAdditionalInfoEnum.NoInfo)
            {
                AudioEventHandler.CallBgmDirectorActionTriggered(triggerAction, additionalInfo);
            }
            else
            {
                switch (additionalInfo)
                {
                    case ActionAdditionalInfoEnum.BgmStartTimeSeconds:
                        AudioEventHandler.CallBgmDirectorActionTriggered(triggerAction, additionalInfo, bgmStartTimeSeconds);
                        break;
                    default:
                        AudioEventHandler.CallBgmDirectorActionTriggered(triggerAction, additionalInfo);
                        break;
                }
            }

            _isUsed=true;
        }

        private void OnHandleEchoInteraction()
        {
            ExecuteTriggerAction();
        }

        private bool BgmStartTimeSecondsShow()
        {
            return triggerAction == BgmDirectorTriggerActionEnum.PlayBgmA|| triggerAction == BgmDirectorTriggerActionEnum.PlayBgmB;
        }

    }
}