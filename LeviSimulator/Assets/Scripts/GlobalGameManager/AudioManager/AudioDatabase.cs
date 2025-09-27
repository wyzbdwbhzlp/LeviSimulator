using UnityEngine;
using System.Collections.Generic;
using Game.Audio;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

namespace Game.Audio
{
    /// <summary>
    /// 音频数据项，包含音频剪辑和相关配置
    /// </summary>
    [System.Serializable]
    public class AudioData
    {
        [Header("基础信息")]
        public string audioName;                    // 音频名称
        public AudioClip audioClip;                 // 音频剪辑
        public AudioChannel defaultChannel = AudioChannel.SFX; // 默认通道
        
        [Header("播放配置")]
        [Range(0f, 1f)]
        public float volume = 1f;                   // 音量
        
        [Range(0.1f, 3f)]
        public float pitch = 1f;                    // 音调
        
        [Range(0f, 1f)]
        public float spatialBlend = 0f;             // 空间混合（0=2D, 1=3D）
        
        public bool isLoop = false;                 // 是否循环播放
        public float throttleInterval = 0f;         // 节流间隔（秒）
        
        [Header("淡入淡出")]
        public bool supportsFade = false;           // 是否支持淡入淡出
        public float defaultFadeDuration = 0.5f;    // 默认淡入淡出时间

        [Header("分类标签")]
        public string category = "";                // 音频分类（如：UI、Player、Environment等）
        public List<string> tags = new List<string>(); // 标签列表
    }

    /// <summary>
    /// 音频配置数据库 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "AudioDatabase", menuName = "Game/Audio/Audio Database", order = 1)]
    public class AudioDatabase : ScriptableObject
    {
        [Header("音频数据库")]
        [TableList]public List<AudioData> audioDataList = new List<AudioData>();
        
        [Header("分类配置")]
        public List<string> categories = new List<string>() { "BGM", "SFX", "UI", "Player", "Environment" };
        
        [Header("数据库信息")]
        public string databaseVersion = "1.0";
        public System.DateTime lastUpdated;

        [Button("生成音频名称枚举")]
        public void GenerateAudioNameEnums()
        {
            string enumPath = "Assets/Scripts/GlobalGameManager/AudioManager/AudioNames.cs";
            using (var writer = new System.IO.StreamWriter(enumPath, false))
            {
                writer.WriteLine("namespace Game.Audio");
                writer.WriteLine("{");
                writer.WriteLine("    public enum AudioNames");
                writer.WriteLine("    {");
                foreach (var audioData in audioDataList)
                {
                    string enumName = audioData.audioName.Replace(" ", "_").Replace("-", "_");
                    writer.WriteLine($"        {enumName},");
                }
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
            Debug.Log($"音频名称枚举已生成，路径：{enumPath}");
        }

        /// <summary>
        /// 根据名称获取音频数据
        /// </summary>
        public AudioData GetAudioData(string audioName)
        {
            var audiodata= audioDataList.Find(data => data.audioName == audioName);
            if (audiodata == null)
            {
                Debug.LogWarning($" Audio '{audioName}'未找到");
            }
            return audiodata;
        }
        /// <summary>
        ///  根据枚举获取音频数据
        /// </summary>
        /// <param name="audioName"></param>
        /// <returns></returns>
        public AudioData GetAudioData(AudioNames audioName)
        { 
            int index=(int)audioName;
            if (index >= 0 && index < audioDataList.Count)
            {
                return audioDataList[index];
            }
            else
            {
                Debug.LogWarning($" Audio '{audioName}'未找到");
                return null;
            }
        }

        public AudioData GetAudioData(string audioName, string category)
        {
            var audioDateList=GetAudioDataByCategory(category);
            var audiodata= audioDateList.Find(data => data.audioName == audioName);
            if (audiodata == null)
            {
                Debug.LogWarning($" Audio '{audioName}'未找到");
            }
            return audiodata;

        }

        /// <summary>
        /// 根据分类获取音频数据列表
        /// </summary>
        public List<AudioData> GetAudioDataByCategory(string category)
        {
            return audioDataList.FindAll(data => data.category == category);
        }
#if UNITY_EDITOR
        [Button("从Resources加载音频")]
        public void LoadAudioFromResources(string resourcesPath)
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
            {
                resourcesPath = "Audio";
            }

            resourcesPath = resourcesPath.Trim();

            const string resourcesPrefix = "Resources/";
            if (resourcesPath.StartsWith(resourcesPrefix, System.StringComparison.OrdinalIgnoreCase))
            {
                resourcesPath = resourcesPath.Substring(resourcesPrefix.Length);
            }

            var clips = Resources.LoadAll<AudioClip>(resourcesPath);
            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning($"在Resources/{resourcesPath}下未找到音频资源。");
                return;
            }

            int addedCount = 0;
            foreach (var clip in clips)
            {
                if (clip == null)
                {
                    continue;
                }

                if (audioDataList.Any(data => data.audioClip == clip || data.audioName == clip.name))
                {
                    continue;
                }

                audioDataList.Add(new AudioData
                {
                    audioName = clip.name,
                    audioClip = clip
                });
                addedCount++;
            }

            lastUpdated = System.DateTime.Now;
            EditorUtility.SetDirty(this);

            Debug.Log($"从Resources/{resourcesPath}加载音频完成，新添加 {addedCount} 条记录，当前总数 {audioDataList.Count}。");
        }
#endif
    }

}
