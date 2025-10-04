using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Utilities;

namespace Game.Audio
{
    [CreateAssetMenu(fileName = "BgmDirectorData", menuName = "Audio/BgmDirectorData", order = 1)]
    public class BgmDirectorScriptableObject : ScriptableObject
    {
        [SerializeField]private List<BgmData> bgmDataList = new List<BgmData>();
        [SerializeField]private List<BgmDirectorData> bgmDirectorDataList = new List<BgmDirectorData>();
        private List<FieldInfo> _bgmDiretorContextFieldInfo;

        public BgmDirectorData GetBgmDirectorDataByPoint(BgmDirectorTriggerPointEnum point)
        {
            var data = bgmDirectorDataList.FindAll(d => d.TriggerPoint == point);
            if (data.Count > 1)
            {
                LogUtil.LogWarning($"存在多个 '{point}' 的BGM触发点，默认返回第一个");
            }
            var firstData = data.Count > 0 ? data[0] : null;
            if (firstData == null)
            {
                LogUtil.LogWarning($"未找到 '{point}' 的BGM触发点");
                return null;
            }
            return firstData;
        }
        public AudioClip GetBgmClipByName(string name)
        {
            var data = bgmDataList.Find(d => d.bgmName.Equals(name));
            if (data == null)
            {
                Debug.LogWarning($"BGM '{name}'未找到");
                return null;
            }
            return data.clip;
        }
        public void ModifyBgmClipByTriggerPoint(BgmDirectorTriggerPointEnum point, string newBgmName)
        {
            var data = bgmDirectorDataList.Find(d => d.TriggerPoint == point);
            if (data == null)
            {
                LogUtil.LogWarning($"未找到 '{point}' ");
                return;
            }
            data.Context.bgmName = newBgmName;
        }
    }

    [Serializable]
    public class BgmData
    {
        public string bgmName;
        public AudioClip clip;
    }
}