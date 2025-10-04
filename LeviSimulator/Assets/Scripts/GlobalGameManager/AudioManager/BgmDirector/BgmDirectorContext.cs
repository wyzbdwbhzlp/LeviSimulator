using System;
using UnityEngine.Events;

namespace Game.Audio
{
    [System.Serializable]
    public class BgmDirectorContext
    {
        public string bgmName;
        public float bgmFadeDuration=3f;
        public float bgmStartTimeSeconds;
        public UnityEvent onBgmStart;
    }
}