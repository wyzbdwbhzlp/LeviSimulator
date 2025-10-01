using CollectibleEcho;
using HUD;
using PlayerControllers.Refactored.Systems;
using Sirenix.OdinInspector;
using UIManager;
using UnityEngine;
using Utilities;

namespace SceneInteractionObject
{
    public class EchoInteractable : MonoBehaviour
    {
        [SerializeField][LabelText("欲展示的回声ID")] private int echoId;
        [SerializeField][ReadOnly]private bool _playerInRange = false;
        
        public event System.Action OnEchoInteracted;
        private PlayerInputSystem _playerInputSystem;

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

        // private void Update()
        // {
        //     if (_playerInRange&& Input.GetKeyDown(KeyCode.E))
        //     {
        //         InteractWithEcho();
        //     }
        // }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = true;
                ShowInteractionPrompt();
                _playerInputSystem= other.GetComponent<PlayerInputSystem>();
                _playerInputSystem.RegisterInteractCallback(InteractWithEcho);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInRange = false;
                HideInteractionPrompt();
                _playerInputSystem.UnregisterInteractCallback();
            }
        }

        private void InteractWithEcho()
        {
            CollectibleEchoController.Instance.OpenEcho(echoId);
            OnEchoInteracted?.Invoke();
            HideInteractionPrompt();
        }

        private void ShowInteractionPrompt()
        {
            MainUIManager.ShowHUDComponent<InteractionHUD>();
        }

        private void HideInteractionPrompt(int id=0)
        {
            MainUIManager.HideHUDComponent<InteractionHUD>();
        }
    }
}