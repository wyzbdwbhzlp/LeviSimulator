using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Game.Audio;
using GlobalGameManager;
using UIManager;
using Utilities;

namespace GlobalGameManager
{
    public class GlobalManager : MonoBehaviour
    {
        [SerializeField]private SceneEnum initialScene;
        
        
        
        private static GlobalManager _instance;

        public static GlobalManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GlobalManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GlobalManager");
                        _instance = go.AddComponent<GlobalManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        [Header("管理器引用")]
        public SceneLoadManager sceneLoadManager;
        public PlayerSpawnManager playerSpawnManager;
        public GameStateManager gameStateManager;
        public MainUIManager mainUIManager;
        public TimeManager timeManager;
        public AudioHub audioHub;
        

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManagers();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeManagers()
        {
            // 初始化各个管理器
            if (sceneLoadManager == null)
                sceneLoadManager = gameObject.AddComponent<SceneLoadManager>();

            if (playerSpawnManager == null)
                playerSpawnManager = gameObject.AddComponent<PlayerSpawnManager>();

            if (gameStateManager == null)
                gameStateManager = gameObject.AddComponent<GameStateManager>();
            
            mainUIManager=MainUIManager.Instance;
            if(mainUIManager==null)
                mainUIManager= gameObject.AddComponent<MainUIManager>();
            if (timeManager == null)
                timeManager = gameObject.AddComponent<TimeManager>();
            if (audioHub == null)
            {
                LogUtil.LogError(" AudioHub 未设置");
            }

            gameStateManager.Initialize();
            playerSpawnManager.Initialize(gameStateManager);
            mainUIManager.Initialize();
            timeManager.Initialize();
            audioHub.Initialize();

            gameStateManager.SubscribeToEvents(this);
        }
        private IEnumerator Start()
        {
            // 等待一帧，确保其它系统完成 Awake/OnEnable
            yield return null;
            if (sceneLoadManager != null)
            {
                // 首先加载HUD场景，并保持它不被卸载
                sceneLoadManager.LoadScene(SceneEnum.HUDScene, false);
                // 然后加载初始游戏场景
                sceneLoadManager.LoadScene(initialScene);
            }
        }
    }
}