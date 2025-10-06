using System;
using Game.Audio;
using GlobalGameManager;
using HUD;
using PlayerControllers.Refactored.Systems;
using Sirenix.OdinInspector;
using UIManager;
using UnityEngine;
using Utilities;

namespace SceneInteractionObject
{
    public class CheckPointItem :MonoBehaviour
    {
        private bool _canInteract = false;
        private GameObject _player;
        private PlayerInputSystem _playerInputSystem;

        private void Awake()
        {
            var checkPointcollider = GetComponent<Collider>();
            if (checkPointcollider == null)
            {
                LogUtil.LogError("CheckPointItem 需要一个 Collider 组件来检测玩家的进入和离开。");
            }
            else
            {
                checkPointcollider.isTrigger = true; //tip 确保 Collider 设置为 Trigger
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var interactionHUD = MainUIManager.ShowHUDComponent<InteractionHUD>();
                InteractionData interactionData = new InteractionData("按下[E]键保存游戏进度");
                interactionHUD.UpdateHUDData(interactionData);
                _player = other.gameObject;
                _playerInputSystem = _player.GetComponent<PlayerInputSystem>();
                _playerInputSystem.RegisterInteractCallback(OnInteractButtonPressed);
                _canInteract = true;
                EventBroadcaster.CallPlayerEnterCheckPoint();//广播进入存档点
            }
        } 

        // private void Update()
        // {
        //     if (_canInteract && Input.GetKeyDown(KeyCode.E))
        //     {
        //         OnInteractButtonPressed();
        //     }
        // }

        [Button("测试存档点交互")]
        private void OnInteractButtonPressed()
        {
            MainUIManager.ShowHUDComponent<FeedbackBannerHUD>().UpdateHUDData("游戏进度已保存");
            AudioEventHandler.CallPlayOneShotFor2D(AudioNames.存档提示音效);
            EventBroadcaster.CallUpdatePlayerCheckPoint(_player.transform.position, _player.transform.rotation);
            EventBroadcaster.CallPlayerSaved(_player.transform.position, _player.transform.rotation);
            LogUtil.Log("游戏进度已保存");
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                MainUIManager.HideHUDComponent<InteractionHUD>();
                _playerInputSystem.UnregisterInteractCallback();
                _player = null;
                _canInteract = false;
                EventBroadcaster.CallPlayerExitCheckPoint();//广播离开存档点
            }
        }
        
    }
}