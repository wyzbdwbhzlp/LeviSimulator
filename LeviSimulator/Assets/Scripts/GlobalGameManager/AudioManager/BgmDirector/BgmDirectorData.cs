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
        [SerializeField]private BgmDirectorEnum _triggerAction;
        [LabelText("追加上下文")]
        [SerializeField]private BgmDirectorContext _context;

        public BgmDirectorEnum TriggerAction => _triggerAction;
        public BgmDirectorTriggerPointEnum TriggerPoint => _triggerPoint;
        public BgmDirectorContext Context => _context;
    }
}