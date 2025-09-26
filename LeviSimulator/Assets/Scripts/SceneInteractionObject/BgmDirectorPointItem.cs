using Game.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace SceneInteractionObject
{
    [RequireComponent(typeof(Collider))]
    public class BgmDirectorPointItem:MonoBehaviour
    {
        [LabelText("触发动作")]
        [SerializeField]private BgmDirectorTriggerActionEnum triggerAction=BgmDirectorTriggerActionEnum.NoAction;

        [LabelText("附加信息")] [SerializeField]
        private ActionAdditionalInfoEnum additionalInfo = ActionAdditionalInfoEnum.NoInfo;
        [LabelText("BGM开始时间(秒)")]
        [ShowIf("BgmStartTimeSecondsShow")][SerializeField]private float bgmStartTimeSeconds;
        
        private bool _isUsed=false;
        private void Awake()
        {
            var _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }
        private void OnTriggerEnter(Collider other)
        {
            if(triggerAction== BgmDirectorTriggerActionEnum.NoAction) return;
            if (other.CompareTag("Player"))
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
        }

        private bool BgmStartTimeSecondsShow()
        {
            return triggerAction == BgmDirectorTriggerActionEnum.PlayBgmA|| triggerAction == BgmDirectorTriggerActionEnum.PlayBgmB;
        }

    }
}