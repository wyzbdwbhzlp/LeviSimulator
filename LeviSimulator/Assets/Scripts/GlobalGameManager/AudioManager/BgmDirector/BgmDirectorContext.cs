using System;
using UnityEngine.Events;

namespace Game.Audio
{
    [System.Serializable]
    public class BgmDirectorContext
    {
        public string bgmName;
        public float bgmStartTimeSeconds;
        public UnityEvent onBgmStart;
    }
}