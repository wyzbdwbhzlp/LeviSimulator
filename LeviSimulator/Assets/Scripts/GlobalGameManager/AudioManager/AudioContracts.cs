using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// 音频通道枚举
    /// </summary>
    public enum AudioChannel
    {
        BGM,  // 背景音乐
        SFX,  // 音效
        UI    // 界面音效
    }

    /// <summary>
    /// 可循环播放的音频句柄
    /// </summary>
    public interface IAudioHandle
    {
        /// <summary>是否正在播放</summary>
        bool IsPlaying { get; }

        /// <summary>音源对象</summary>
        AudioSource Source { get; }

        /// <summary>
        /// 停止播放，可选淡出时间
        /// </summary>
        void Stop(float fadeOut = 0f);
    }

    /// <summary>
    /// 统一的音频播放接口
    /// </summary>
    public interface IAudioPlayer
    {
        /// <summary>
        /// 播放一次性音效（不会循环）
        /// </summary>
        AudioSourceWrapper PlayOneShotFor2D(AudioNames names);

        /// <summary>
        /// 在指定位置播放音效，可选择 2D / 3D 声效（spatialBlend 控制）
        /// </summary>
        AudioSourceWrapper PlayOneShotFor3D(AudioNames names, Vector3 position);

        /// <summary>
        /// 播放循环音效（返回可控制的句柄）
        /// </summary>
        IAudioHandle PlayLoop(AudioClip clip, AudioChannel channel = AudioChannel.SFX, float volume = 1f, float pitch = 1f, float spatialBlend = 1f);

        /// <summary>
        /// 播放背景音乐（支持淡入）
        /// </summary>
        void PlayBGM(AudioClip clip, float fadeSeconds = 0.75f, float targetVolume = 1f);

        /// <summary>
        /// 停止背景音乐（支持淡出）
        /// </summary>
        void StopBGM(float fadeSeconds = 0.5f);

        /// <summary>
        /// 停止某个通道上的所有音频
        /// </summary>
        void StopAllOnChannel(AudioChannel channel);
    }
}
