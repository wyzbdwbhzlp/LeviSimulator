using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;
using Utilities;

namespace Game.Audio
{
    /// <summary>
    /// 音效系统总管：负责对象池、通道管理、配额限制、BGM 淡入淡出。
    /// </summary>
    public class AudioHub : Singleton<AudioHub>, IAudioPlayer
    {
        public static AudioHub Instance { get; private set; }

        [Header("基础设置")]
        [SerializeField] private GameObject audioPrefab;   // 音频对象预制体
        [SerializeField] private int initPoolSize = 16;    // 初始对象池大小
        [SerializeField] private bool dontDestroyOnLoad = true; // 是否在切换场景时保留
        [SerializeField] private AudioDatabase audioDatabase; // 音频数据库

        [Header("混音组")]
        public AudioMixerGroup bgmGroup;  // 背景音乐混音组
        public AudioMixerGroup sfxGroup;  // 音效混音组
        public AudioMixerGroup uiGroup;   // UI 音效混音组

        [Header("通道上限")]
        public int sfxVoices = 16;  // SFX 通道同时能播放的音效数
        public int uiVoices = 8;    // UI 通道同时能播放的音效数

        private Queue<AudioSourceWrapper> pool = new Queue<AudioSourceWrapper>(); // 音效对象池
        private Dictionary<AudioChannel, LinkedList<AudioSourceWrapper>> activeByChannel;

        // 节流限制：避免按钮音效频繁触发
        private Dictionary<AudioClip, float> lastPlayedTime = new Dictionary<AudioClip, float>();

        // BGM 双通道，用于淡入淡出
        private AudioSource bgmA, bgmB;
        private bool bgmAIsActive = true;
        private Coroutine bgmCrossRoutine;

        public void Initialize()
        {
            if( audioPrefab == null)
                LogUtil.LogError(" audioPrefab 未设置，请检查 Inspector 配置");
            if( audioDatabase == null)
                LogUtil.LogError(" audioDatabase 未设置，请检查 Inspector 配置");
            
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
            
            
            // 初始化对象池
            for (int i = 0; i < initPoolSize; i++) CreateNew();

            // 初始化活动列表
            activeByChannel = new Dictionary<AudioChannel, LinkedList<AudioSourceWrapper>>
            {
                { AudioChannel.SFX, new LinkedList<AudioSourceWrapper>() },
                { AudioChannel.UI, new LinkedList<AudioSourceWrapper>() }
            };

            // 初始化 BGM 双源
            bgmA = CreateBgmSource("BGM_A");
            bgmB = CreateBgmSource("BGM_B");
        }
        private void OnEnable()
        {
            SubscribeToGameEvents();
        }
        private void OnDisable()
        {
            UnsubscribeFromGameEvents();
        }
        private void SubscribeToGameEvents()
        {
           AudioEventHandler.PlayOneShotFor2D += PlayOneShotFor2D;
           AudioEventHandler.PlayOneShotFor3D += PlayOneShotFor3D;
           AudioEventHandler.PlayLoop += PlayLoop;
           AudioEventHandler.PlayBGM += PlayBGM;
           AudioEventHandler.StopBGM += StopBGM;
           AudioEventHandler.StopAllOnChannel += StopAllOnChannel;
        }
        private void UnsubscribeFromGameEvents()
        {
            AudioEventHandler.PlayOneShotFor2D -= PlayOneShotFor2D;
            AudioEventHandler.PlayOneShotFor3D -= PlayOneShotFor3D;
            AudioEventHandler.PlayLoop -= PlayLoop;
            AudioEventHandler.PlayBGM -= PlayBGM;
            AudioEventHandler.StopBGM -= StopBGM;
            AudioEventHandler.StopAllOnChannel -= StopAllOnChannel;
        }

        private AudioSource CreateBgmSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.loop = true;
            src.playOnAwake = false;
            src.outputAudioMixerGroup = bgmGroup;
            src.spatialBlend = 0f;
            return src;
        }

        private AudioSourceWrapper CreateNew()
        {
            var go = Instantiate(audioPrefab, transform);
            var wrapper = go.GetComponent<AudioSourceWrapper>();
            wrapper.Init(this);
            go.SetActive(false);
            pool.Enqueue(wrapper);
            return wrapper;
        }

        private AudioSourceWrapper GetFromPool()
        {
            if (pool.Count == 0) CreateNew();
            return pool.Dequeue();
        }

        public void ReturnToPool(AudioSourceWrapper wrapper)
        {
            wrapper.gameObject.SetActive(false);
            pool.Enqueue(wrapper);
        }

        // ----------- Active 管理（通道上的活跃对象） -----------

        public void NotifyActive(AudioSourceWrapper wrapper, AudioChannel channel)
        {
            if (channel == AudioChannel.BGM) return;
            var list = activeByChannel[channel];
            list.AddLast(wrapper);

            int limit = (channel == AudioChannel.SFX) ? sfxVoices : uiVoices;
            if (list.Count > limit)
            {
                // 超出上限时，强制停止最早的音源
                list.First.Value.Stop();
            }
        }

        public void NotifyInactive(AudioSourceWrapper wrapper, AudioChannel channel)
        {
            if (channel == AudioChannel.BGM) return;
            activeByChannel[channel].Remove(wrapper);
        }

        // ----------- IAudioPlayer 实现 -----------

        //throttleInterval指定节流时间间隔，单位秒，防止同一音效被频繁触发
        /// <summary>
        ///  播放一次性音效（不会循环），用于UI音效，无需指定position
        /// </summary>
        /// <param name="audioNames"></param>
        /// <param name="clip"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="throttleInterval"></param>
        [Button(" 测试UI音效 ")]
        public AudioSourceWrapper PlayOneShotFor2D(AudioNames audioNames)
        {
            
            var data = audioDatabase.GetAudioData(audioNames);
            if (data == null)
            {
                Debug.LogWarning($"AudioHub: 未找到音频数据 {audioNames}");
                return null;
            }
            var clip = data.audioClip;
            float volume = data.volume;
            float pitch = data.pitch;
            float throttleInterval = data.throttleInterval;
            
            if (!clip) return null; // 空引用检查
            if (IsThrottled(clip, throttleInterval)) return null; // 节流

            var wrapper = GetFromPool();
            var group = GetGroup(AudioChannel.UI);
            
            wrapper.PlayOneShot(clip, volume, pitch, AudioChannel.UI, group, null, 0f); 
            return wrapper;
        }

        /// <summary>
        ///   播放一次性音效（不会循环），用于场景实际音效，需要指定position
        /// </summary>
        /// <param name="audioNames"></param>
        /// <param name="pos"></param>
        /// <param name="clip"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="spatialBlend"></param>
        /// <param name="throttleInterval"></param>
        /// <param name="channel"></param>
        [Button(" 测试3D音效 ")]
        public AudioSourceWrapper PlayOneShotFor3D(AudioNames audioNames, Vector3 pos
            // AudioClip clip, Vector3 pos, float volume = 1f,
            // float pitch = 1f, float spatialBlend = 1f,
            // float throttleInterval = 0f,
            // AudioChannel channel = AudioChannel.SFX
        )
        {
            var data = audioDatabase.GetAudioData(audioNames); 
            if (data == null)
            {
                Debug.LogWarning($"AudioHub: 未找到音频数据 {audioNames}");
                return null;
            }
            var clip = data.audioClip;
            float volume = data.volume;
            float pitch = data.pitch;
            float spatialBlend = data.spatialBlend;
            float throttleInterval = data.throttleInterval;
            AudioChannel channel = data.defaultChannel;
            
            if (!clip) return null;
            if (IsThrottled(clip, throttleInterval)) return null;

            var wrapper = GetFromPool();
            var group = GetGroup(channel);
            wrapper.PlayOneShot(clip, volume, pitch, channel, group, pos, spatialBlend);
            return wrapper;
        }

        /// <summary>
        ///  播放循环音效，通常用于环境音效或持续性音效
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="channel"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        /// <param name="spatialBlend"></param>
        /// <returns></returns>
        public IAudioHandle PlayLoop(AudioClip clip, AudioChannel channel = AudioChannel.SFX, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
        {
            if (!clip) return null;
            var wrapper = GetFromPool();
            var group = GetGroup(channel);
            wrapper.PlayLoop(clip, volume, pitch, channel, group, null, spatialBlend);
            return wrapper;
        }

        public void StopAllOnChannel(AudioChannel channel)
        {
            if (channel == AudioChannel.BGM) { StopBGM(); return; }
            var list = activeByChannel[channel];
            foreach (var w in new List<AudioSourceWrapper>(list))
                w.Stop();
            list.Clear();
        }

        // ----------- BGM 淡入淡出 -----------

        [Button("播放Bgm")]
        public void PlayBGM(AudioClip clip, float fadeSeconds = 0.75f, float targetVolume = 1f)
        {
            if (!clip) return;

            var from = bgmAIsActive ? bgmA : bgmB;
            var to = bgmAIsActive ? bgmB : bgmA;
            bgmAIsActive = !bgmAIsActive;

            to.clip = clip;
            to.volume = 0f;
            to.Play();

            if (bgmCrossRoutine != null) StopCoroutine(bgmCrossRoutine);
            bgmCrossRoutine = StartCoroutine(CrossFade(from, to, fadeSeconds, targetVolume));
        }

        public void StopBGM(float fadeSeconds = 0.5f)
        {
            var active = bgmAIsActive ? bgmA : bgmB;
            if (bgmCrossRoutine != null) StopCoroutine(bgmCrossRoutine);
            StartCoroutine(FadeOutAndStop(active, fadeSeconds));
        }

        private IEnumerator CrossFade(AudioSource from, AudioSource to, float dur, float targetVol)
        {
            float t = 0f;
            float fromStart = from ? from.volume : 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = t / dur;
                if (from) from.volume = Mathf.Lerp(fromStart, 0f, k);
                to.volume = Mathf.Lerp(0f, targetVol, k);
                yield return null;
            }
            if (from) from.Stop();
            to.volume = targetVol;
        }

        private IEnumerator FadeOutAndStop(AudioSource src, float dur)
        {
            float start = src.volume;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / dur);
                yield return null;
            }
            src.Stop();
            src.volume = start;
        }

        // ----------- 工具函数 -----------

        private AudioMixerGroup GetGroup(AudioChannel ch)
        {
            switch (ch)
            {
                case AudioChannel.BGM: return bgmGroup;
                case AudioChannel.UI: return uiGroup ? uiGroup : sfxGroup;
                default: return sfxGroup;
            }
        }

        private bool IsThrottled(AudioClip clip, float interval)
        {
            if (interval <= 0f) return false;
            float now = Time.unscaledTime;
            if (lastPlayedTime.TryGetValue(clip, out var last) && now - last < interval) return true;
            lastPlayedTime[clip] = now;
            return false;
        }
    }
}
