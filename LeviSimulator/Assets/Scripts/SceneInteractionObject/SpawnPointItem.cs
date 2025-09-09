using System;
using GlobalGameManager;
using UnityEngine;
using Utilities;

namespace SceneInteractionObject
{
    public class SpawnPointItem:MonoBehaviour
    {
        private void Start()
        {
            EventBroadcaster.CallPlayerReadySpawn(transform.position,transform.rotation);
        }
    }
}