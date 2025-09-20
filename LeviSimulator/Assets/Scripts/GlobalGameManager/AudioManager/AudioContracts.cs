using UnityEngine;
using UnityEngine.Audio;

namespace Game.Audio
{
    /// <summary>
    /// 音频通道分类
    /// </summary>
    public enum AudioChannel
    {
        BGM,
        SFX,
        UI
    }
    ///<summary>
    ///可循环播放的句柄
    ///</summary>
    public interface IAudioHandle
    {
        bool IsPlaying { get; }
        AudioSource Source { get; }
        void Stop(float fadeOut = 0f);
    }
    ///<summary>///
    ///对外统一的音频播放接口
    ///</summary>
    public interface IAudioPlayer
    {
        //一次性短音效
        void PlayOneShot(AudioClip clip, float volume = 1f, AudioChannel channel = AudioChannel.SFX, float pitch = 1f, float throttleInterval = 0f);
       //世界坐标2d/3d音效 spatialblend为2d/3d混合
        void PlayAtPosition(AudioClip clip, Vector3 pos, float volume = 1f, AudioChannel channel = AudioChannel.SFX, float pitch = 1f, float spatialBlend = 1f, float throttleInterval = 0f);
        //循环声
        IAudioHandle PlayLoop(AudioClip clip, AudioChannel channel = AudioChannel.SFX, float volume = 1f, float pitch = 1f, float spatialBlend = 1f);

       //切换 播放bgm
        void PlayBGM(AudioClip clip, float fadeSeconds = 0.75f, float targetVolume = 1f);
       //停止bgm
        void StopBGM(float fadeSeconds = 0.5f);

        //停掉某个通道上的所有声音
        void StopAllOnChannel(AudioChannel channel);
    }
}
