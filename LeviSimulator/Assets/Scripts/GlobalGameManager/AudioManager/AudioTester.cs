using Game.Audio;
using UnityEngine;

public class AudioTester : MonoBehaviour
{
    public AudioClip  bgmA;
    IAudioHandle windLoop;

    void Start()
    {
        AudioHub.Instance.PlayBGM(bgmA, 1.0f, 0.8f);
    }
}
