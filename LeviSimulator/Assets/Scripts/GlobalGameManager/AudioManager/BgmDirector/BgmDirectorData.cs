using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Audio
{
    [System.Serializable]
    public class BgmDirectorData
    {
        [LabelText("触发点")]
        [SerializeField]private BgmDirectorTriggerPointEnum  _triggerPoint;
        [LabelText("触发动作")]
        [SerializeField]private BgmDirectorTriggerActionEnum _triggerAction;
        [LabelText("BGM开始时间(秒)")]
        [SerializeField][ShowIf("BgmStartTimeSecondsShow")]private float BgmStartTimeSeconds;

        public BgmDirectorTriggerActionEnum TriggerAction => _triggerAction;
        public BgmDirectorTriggerPointEnum TriggerPoint => _triggerPoint;
        public float BgmStartTime => BgmStartTimeSeconds;

        public bool BgmStartTimeSecondsShow()
        {
            return _triggerAction == BgmDirectorTriggerActionEnum.PlayBgmA|| _triggerAction == BgmDirectorTriggerActionEnum.PlayBgmB;
        }
    }
}