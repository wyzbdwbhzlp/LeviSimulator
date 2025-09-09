using System;
using UnityEngine;
using Utilities;

namespace SceneInteractionObject
{
    public class LevelCheckPointItem :MonoBehaviour
    {
        private GameObject _currentPlayer;
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _currentPlayer = other.gameObject;
                //todo 呼出互动按钮
            }
        }
        private void OnPlayerPressSavePoint()
        {
            if (_currentPlayer != null)
            {
                //todo 存档逻辑
            }
            else
            {
                LogUtil.LogWarning("当前没有玩家在检查点范围内，无法设置存档点");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.gameObject == _currentPlayer)
                {
                    _currentPlayer = null;
                    //todo 收起互动按钮
                }
            }
        }
    }
}