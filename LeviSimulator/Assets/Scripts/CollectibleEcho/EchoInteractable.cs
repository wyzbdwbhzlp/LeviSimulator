using System;
using HUD;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace CollectibleEcho
{
    public class EchoInteractable : MonoBehaviour
    {
        [SerializeField] private int echoId;
        
        [SerializeField][ReadOnly]private bool _playerInRange = false;

        private void OnEnable()
        {
            EventBroadcaster.EchoViewUIOpened+= HideInteractionPrompt;
            EventBroadcaster.EchoViewUIClosed += ShowInteractionPrompt;
        }
        private void OnDisable()
        {
            EventBroadcaster.EchoViewUIOpened -= HideInteractionPrompt;
            EventBroadcaster.EchoViewUIClosed -= ShowInteractionPrompt;
            HideInteractionPrompt();
        }

        private void Update()
        {
            if (_playerInRange&& Input.GetKeyDown(KeyCode.E))
            {
                InteractWithEcho();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
                ShowInteractionPrompt();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
                HideInteractionPrompt();
            }
        }

        private void InteractWithEcho()
        {
            CollectibleEchoController.Instance.OpenEcho(echoId);
            HideInteractionPrompt();
        }

        private void ShowInteractionPrompt()
        {
            MainUIManager.ShowHUDComponent<InteractionHUD>();
        }

        private void HideInteractionPrompt()
        {
            MainUIManager.HideHUDComponent<InteractionHUD>();
        }
    }
}