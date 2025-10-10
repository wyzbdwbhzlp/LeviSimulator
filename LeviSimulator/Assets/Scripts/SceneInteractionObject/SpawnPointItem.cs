using System;
using GlobalGameManager;
using UnityEngine;
using Utilities;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace SceneInteractionObject
{
    public class SpawnPointItem:MonoBehaviour
    {
        private void Start()
        {
            EventBroadcaster.CallPlayerReadySpawn(transform.position,transform.rotation);
            EventBroadcaster.CallUpdatePlayerCheckPoint(transform.position, transform.rotation);
            EventBroadcaster.CallPlayerSaved(transform.position, transform.rotation);
        }
    }
}