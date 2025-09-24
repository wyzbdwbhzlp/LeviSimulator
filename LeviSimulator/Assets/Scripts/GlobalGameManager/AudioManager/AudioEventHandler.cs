using System;
using UnityEngine;
using Utilities;

namespace Game.Audio
{
    public static class AudioEventHandler
    {
        public static event Func<AudioNames,AudioSourceWrapper> PlayOneShotFor2D;
        /// <summary>
        ///  播放一个2D音效
        /// </summary>
        public static AudioSourceWrapper CallPlayOneShotFor2D(AudioNames names)
        {
            return PlayOneShotFor2D?.Invoke(names);
        }
        
        public static event Func<AudioNames,Vector3,AudioSourceWrapper> PlayOneShotFor3D;
        /// <summary>
        ///  在指定世界坐标播放一个3D音效（一次性，不循环）。
        /// </summary>
        /// <param name="names">音频名称（从数据库查找）</param>
        /// <param name="pos">世界坐标</param>
        public static AudioSourceWrapper CallPlayOneShotFor3D(AudioNames names, Vector3 pos)
        {
            return PlayOneShotFor3D?.Invoke(names, pos);
        }
        
        // 循环播放事件（返回可控制句柄）
        public static event Func<AudioClip, AudioChannel, float, float, float, IAudioHandle> PlayLoop;
        /// <summary>
        ///  播放循环音效，常用于环境/持续性音效。
        /// </summary>
        /// <param name="clip">音频剪辑</param>
        /// <param name="channel">播放通道（默认 SFX）</param>
        /// <param name="volume">音量 0-1</param>
        /// <param name="pitch">音调</param>
        /// <param name="spatialBlend">2D/3D 混合（0=2D,1=3D）</param>
        public static IAudioHandle CallPlayLoop(AudioClip clip, AudioChannel channel = AudioChannel.SFX, float volume = 1f, float pitch = 1f, float spatialBlend = 1f)
        {
            return PlayLoop?.Invoke(clip, channel, volume, pitch, spatialBlend);
        }

        // BGM 控制事件
        public static event Action<AudioClip, float, float> PlayBGM;
        /// <summary>
        ///  播放 BGM（支持淡入）。
        /// </summary>
        /// <param name="clip">音乐剪辑</param>
        /// <param name="fadeSeconds">淡入时长</param>
        /// <param name="targetVolume">目标音量</param>
        public static void CallPlayBGM(AudioClip clip, float fadeSeconds = 0.75f, float targetVolume = 1f)
        {
            PlayBGM?.Invoke(clip, fadeSeconds, targetVolume);
        }

        public static event Action<float> StopBGM;
        /// <summary>
        ///  停止 BGM（支持淡出）。
        /// </summary>
        /// <param name="fadeSeconds">淡出时长</param>
        public static void CallStopBGM(float fadeSeconds = 0.5f)
        {
            StopBGM?.Invoke(fadeSeconds);
        }

        public static event Action<AudioChannel> StopAllOnChannel;
        /// <summary>
        ///  停止某通道上的所有音频（BGM 通道会等价于停止 BGM）。
        /// </summary>
        /// <param name="channel">通道</param>
        public static void CallStopAllOnChannel(AudioChannel channel)
        {
            StopAllOnChannel?.Invoke(channel);
        }


    }
}