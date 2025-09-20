using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// 音效源的封装类：用于播放音效，并在结束后归还到对象池。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceWrapper : MonoBehaviour, IAudioHandle
    {
        private AudioSource source;      // Unity 音频源
        private AudioHub hub;            // 音频总管（用于回收自身）
        private AudioChannel channel;    // 当前所属通道
        private bool isLoop;             // 是否循环播放
        private Coroutine lifeRoutine;   // 生命周期协程

        // IAudioHandle 接口实现
        public bool IsPlaying => source && source.isPlaying;
        public AudioSource Source => source;

        /// <summary>
        /// 初始化，与 AudioHub 建立关联
        /// </summary>
        public void Init(AudioHub hubRef)
        {
            hub = hubRef;
            source = GetComponent<AudioSource>();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 播放一次性音效（不会循环）
        /// </summary>
        public void PlayOneShot(AudioClip clip, float volume, float pitch, AudioChannel ch, AudioMixerGroup group, Vector3? worldPos, float spatialBlend)
        {
            ConfigureSource(group, volume, pitch, spatialBlend, loop: false);
            channel = ch;
            isLoop = false;

            if (worldPos.HasValue) transform.position = worldPos.Value;
            gameObject.SetActive(true);

            source.clip = clip;
            source.Play();

            // 开启协程等待播放结束后回收
            lifeRoutine = StartCoroutine(ReturnWhenFinished());
            hub.NotifyActive(this, ch);
        }

        /// <summary>
        /// 播放循环音效（需外部调用 Stop 停止）
        /// </summary>
        public void PlayLoop(AudioClip clip, float volume, float pitch, AudioChannel ch, AudioMixerGroup group, Vector3? worldPos, float spatialBlend)
        {
            ConfigureSource(group, volume, pitch, spatialBlend, loop: true);
            channel = ch;
            isLoop = true;

            if (worldPos.HasValue) transform.position = worldPos.Value;
            gameObject.SetActive(true);

            source.clip = clip;
            source.Play();
            hub.NotifyActive(this, ch);
        }

        /// <summary>
        /// 配置 AudioSource 的参数
        /// </summary>
        private void ConfigureSource(AudioMixerGroup group, float volume, float pitch, float spatialBlend, bool loop)
        {
            source.outputAudioMixerGroup = group;
            source.volume = Mathf.Clamp01(volume);
            source.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
            source.loop = loop;
            source.spatialBlend = Mathf.Clamp01(spatialBlend); // 0=2D, 1=3D
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1f;
            source.maxDistance = 30f;
        }

        /// <summary>
        /// 等待播放结束后回收对象
        /// </summary>
        private IEnumerator ReturnWhenFinished()
        {
            while (source && source.isPlaying) yield return null;
            StopAndReturnImmediate();
        }

        /// <summary>
        /// 停止播放（可选淡出时间）
        /// </summary>
        public void Stop(float fadeOut = 0f)
        {
            if (!source) return;
            if (fadeOut <= 0f) { StopAndReturnImmediate(); return; }
            StartCoroutine(FadeOutThenStop(fadeOut));
        }

        /// <summary>
        /// 执行淡出后停止播放
        /// </summary>
        private IEnumerator FadeOutThenStop(float dur)
        {
            float startVol = source.volume;
            float t = 0f;
            while (t < dur && source)
            {
                t += Time.unscaledDeltaTime;
                source.volume = Mathf.Lerp(startVol, 0f, t / dur);
                yield return null;
            }
            StopAndReturnImmediate();
        }

        /// <summary>
        /// 立即停止并归还到对象池
        /// </summary>
        private void StopAndReturnImmediate()
        {
            if (lifeRoutine != null) StopCoroutine(lifeRoutine);
            if (source) source.Stop();

            hub.NotifyInactive(this, channel);   // 通知 AudioHub 当前音源已停止
            hub.ReturnToPool(this);              // 归还到对象池
        }
    }
}
