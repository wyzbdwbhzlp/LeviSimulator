using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource BgmAudio;
    [SerializeField] AudioSource SfxAudio;

    public AudioClip BgmClip;
    

    private bool isPlaying = false; // 控制音频是否正在播放

    private void Start()
    {
        BgmAudio.clip = BgmClip;
        BgmAudio.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        // 如果 SfxAudio 不在播放中，才可以播放音效
        if (!isPlaying)
        {
            StartCoroutine(PlaySfxCoroutine(clip));
        }
    }

    private IEnumerator PlaySfxCoroutine(AudioClip clip)
    {
        isPlaying = true; // 设置为正在播放
        SfxAudio.PlayOneShot(clip);

        // 等待直到音效播放完成
        yield return new WaitForSeconds(clip.length);

        isPlaying = false; // 播放完成后，允许再次播放
    }
}
