using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Obsolete("使用 AudioHub 替代")]
public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource BgmAudio;
    [SerializeField] AudioSource SfxAudio;

    public AudioClip BgmClip;
   

    private bool isPlaying = false; // ������Ƶ�Ƿ����ڲ���

    private void Start()
    {
        BgmAudio.clip = BgmClip;
        BgmAudio.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        // ��� SfxAudio ���ڲ����У��ſ��Բ�����Ч
        if (!isPlaying)
        {
            StartCoroutine(PlaySfxCoroutine(clip));
        }
    }

    private IEnumerator PlaySfxCoroutine(AudioClip clip)
    {
        isPlaying = true; // ����Ϊ���ڲ���
        SfxAudio.PlayOneShot(clip);

        // �ȴ�ֱ����Ч�������
        yield return new WaitForSeconds(clip.length);

        isPlaying = false; // ������ɺ������ٴβ���
    }
}
