using PlayerControllers.PlayerMovemenSettings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GlobalGameManager
{
    public class ScriptableObjectDistributionManager:Singleton<ScriptableObjectDistributionManager>
    {
        [InlineEditor][SerializeField]private PlayerWalkRunSetting _playerWalkRunSetting;
        public PlayerWalkRunSetting PlayerWalkRunSetting => _playerWalkRunSetting;
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);
        }
        
    }
}