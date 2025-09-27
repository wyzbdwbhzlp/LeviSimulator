using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    [CreateAssetMenu(fileName = "BgmDirectorData", menuName = "Audio/BgmDirectorData", order = 1)]
    public class BgmDirectorScriptableObject : UnityEngine.ScriptableObject
    {
        [SerializeField] private List<BgmDirectorData> bgmDirectorDataList = new List<BgmDirectorData>();

        public BgmDirectorTriggerActionEnum GetBgmDirectorActionByPoint(BgmDirectorTriggerPointEnum point)
        {
            var data = bgmDirectorDataList.Find(d => d.TriggerPoint == point);
            if (data == null)
            {
                return BgmDirectorTriggerActionEnum.NoAction;
            }
            return data.TriggerAction;
        }
    }
}