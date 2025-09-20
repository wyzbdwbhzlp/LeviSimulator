using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// 音效池里的单个“工人”：负责播放音效，并在播放结束后归还给池子。
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceWrapper : MonoBehaviour, IAudioHandle
    {
        private AudioSource source;      // Unity 的音源
        private AudioHub hub;            // 引用总管，用来归还自己
        private AudioChannel channel;    // 当前所属通道
        private bool isLoop;             // 是否循环播放
        private Coroutine lifeRoutine;   // 控制生命周期的协程

        // IAudioHandle接口实现
        public bool IsPlaying => source && source.isPlaying;
        public AudioSource Source => source;

        /// <summary>初始化（由 AudioHub 调用）。</summary>
        public void Init(AudioHub hubRef)
        {
            hub = hubRef;
            source = GetComponent<AudioSource>();
            gameObject.SetActive(false);
        }

        /// <summary>播放一次性音效（不循环）。</summary>
        public void PlayOneShot(AudioClip clip, float volume, float pitch, AudioChannel ch, AudioMixerGroup group, Vector3? worldPos, float spatialBlend)
        {
            ConfigureSource(group, volume, pitch, spatialBlend, loop: false);
            channel = ch;
            isLoop = false;

            if (worldPos.HasValue) transform.position = worldPos.Value;
            gameObject.SetActive(true);

            source.clip = clip;
            source.Play();

            // 协程等待播放完毕 → 回收
            lifeRoutine = StartCoroutine(ReturnWhenFinished());
            hub.NotifyActive(this, ch);
        }

        /// <summary>循环播放（外部需要 Stop）。</summary>
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

        private IEnumerator ReturnWhenFinished()
        {
            while (source && source.isPlaying) yield return null;
            StopAndReturnImmediate();
        }

        public void Stop(float fadeOut = 0f)
        {
            if (!source) return;
            if (fadeOut <= 0f) { StopAndReturnImmediate(); return; }
            StartCoroutine(FadeOutThenStop(fadeOut));
        }

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

        private void StopAndReturnImmediate()
        {
            if (lifeRoutine != null) StopCoroutine(lifeRoutine);
            if (source) source.Stop();

            hub.NotifyInactive(this, channel);   // 告诉总管：我下线了
            hub.ReturnToPool(this);              // 真正回池
        }
    }
}
